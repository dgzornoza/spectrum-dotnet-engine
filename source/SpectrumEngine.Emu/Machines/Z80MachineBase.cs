﻿namespace SpectrumEngine.Emu;

/// <summary>
/// This class is intended to be a reusable base class for emulators using the Z80 CPU.
/// </summary>
public abstract class Z80MachineBase : 
    IZ80Machine, 
    IDebugSupport
{
    /// <summary>
    /// The folder where the ROM files are stored
    /// </summary>
    public const string ROM_RESOURCE_FOLDER = "Roms";

    /// <summary>
    /// This property stores the execution context where the emulated machine runs its execution loop.
    /// </summary>
    public ExecutionContext ExecutionContext { get; } = new();

    /// <summary>
    /// Get the Z80 instance associated with the machine.
    /// </summary>
    public IZ80Cpu Cpu { get; } = new Z80Cpu();

    /// <summary>
    /// Get the base clock frequency of the CPU. We use this value to calculate the machine frame rate.
    /// </summary>
    public int BaseClockFrequency { get; set; }

    /// <summary>
    /// This property gets or sets the value of the current clock multiplier.
    /// </summary>
    /// <remarks>
    /// By default, the CPU works with its regular (base) clock frequency; however, you can use an integer clock
    /// frequency multiplier to emulate a faster CPU.
    /// </remarks>
    public int ClockMultiplier { get; set; }

    /// <summary>
    /// This method provides a way to configure (or reconfigure) the emulated machine after changing the properties
    /// of its components.
    /// </summary>
    public virtual void Configure()
    {
        // --- Implement this method in derived classes
    }

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public virtual void HardReset()
    {
        Cpu.HardReset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public virtual void Reset()
    {
        Cpu.Reset();
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <remarks>
    /// Override in derived classes to implement hardware components running parallel with the CPU.
    /// </remarks>
    protected abstract void OnTactIncremented();

    /// <summary>
    /// Get the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected abstract string DefaultRomResource { get; }

    /// <summary>
    /// Load the specified ROM from the current assembly resource.
    /// </summary>
    /// <param name="romName">Name of the ROM file to load</param>
    /// <param name="page">Optional ROM page for multi-rom machines</param>
    /// <returns>The byte array that represents the ROM contents</returns>
    /// <exception cref="InvalidOperationException">
    /// The ROM cannot be loaded from the named resource.
    /// </exception>
    public static byte[] LoadRomFromResource(string romName, int page = -1)
    {
        var resourceName = page == -1 ? romName : $"{romName}-{page}";
        var currentAsm = typeof(Z80MachineBase).Assembly;
        resourceName = $"{currentAsm.GetName().Name}.{ROM_RESOURCE_FOLDER}.{romName}.{resourceName}.rom";
        var resMan = currentAsm.GetManifestResourceStream(resourceName);
        if (resMan == null)
        {
            throw new InvalidOperationException($"Input stream for the '{romName}' .rom file not found.");
        }
        using var stream = new StreamReader(resMan).BaseStream;
        stream.Seek(0, SeekOrigin.Begin);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// This member stores the last startup breakpoint to check. It allows setting a breakpoint to the first
    /// instruction of a program.
    /// </summary>
    public ushort? LastStartupBreakpoint { get; set; }

    /// <summary>
    /// Executes the machine loop using the current execution context.
    /// </summary>
    /// <returns>
    /// The value indicates the termination reason of the loop. 
    /// </returns>
    public virtual LoopTerminationMode ExecuteMachineLoop()
    {
        // --- Sign that the loop execution is in progress
        ExecutionContext.LastTerminationReason = null;

        // --- Check the startup breakpoint
        if (Cpu.Regs.PC != LastStartupBreakpoint)
        {
            // --- Check startup breakpoint
            CheckBreakpoints();
            if (ExecutionContext.LastTerminationReason.HasValue)
            {
                // --- The code execution has stopped at the startup breakpoint.
                // --- Sign that fact so that the next time the code do not stop
                LastStartupBreakpoint = Cpu.Regs.PC;
                return ExecutionContext.LastTerminationReason.Value;
            }
        }

        // --- Remove the startup breakpoint
        LastStartupBreakpoint = null;

        // --- Execute the machine loop until the frame is completed or the loop is interrupted because of any other
        // --- completion reason, like reaching a breakpoint, etc.
        do
        {
            // --- Test if the machine frame has just been completed.
            if (Cpu.FrameCompleted)
            {
                // --- Update the CPU's clock multiplier, if the machine's has changed.
                var clockMultiplierChanged = false;
                if (AllowCpuClockChange() && Cpu.ClockMultiplier != ClockMultiplier)
                {
                    // --- Use the current clock multiplier
                    Cpu.ClockMultiplier = ClockMultiplier;
                    clockMultiplierChanged = true;
                }

                // --- Allow a machine to handle frame initialization
                OnInitNewFrame(clockMultiplierChanged);
                Cpu.ResetFrameCompletedFlag();
            }

            // --- Set the interrupt signal, if required so
            if (ShouldRaiseInterrupt())
            {
                Cpu.SignalFlags |= Z80Cpu.Z80Signals.Int;
            }
            else
            {
                Cpu.SignalFlags &= ~Z80Cpu.Z80Signals.Int;
            }

            // --- Execute the next CPU instruction entirely 
            do
            {
                Cpu.ExecuteCpuCycle();
            } while (Cpu.Prefix != Z80Cpu.OpCodePrefix.None);

            // --- Allow the machine to do additional tasks after the completed CPU instruction
            AfterInstructionExecuted();

            // --- De the machine reached the termination point?
            if (TestTerminationPoint())
            {
                // --- The machine reached the termination point
                return (ExecutionContext.LastTerminationReason = LoopTerminationMode.UntilExecutionPoint).Value;
            }

            // --- Test if the execution reached a breakpoint
            CheckBreakpoints();
            if (ExecutionContext.LastTerminationReason.HasValue)
            {
                // --- The code execution has stopped at the startup breakpoint.
                // --- Sign that fact so that the next time the code do not stop
                LastStartupBreakpoint = Cpu.Regs.PC;
                return ExecutionContext.LastTerminationReason.Value;
            }

            // --- Test HALT mode
            if (ExecutionContext.LoopTerminationMode == LoopTerminationMode.UntilHalt && Cpu.Halted)
            {
                // --- The CPU is halted
                return (ExecutionContext.LastTerminationReason = LoopTerminationMode.UntilHalt).Value;
            }


        } while (!Cpu.FrameCompleted);

        // --- Done
        return (ExecutionContext.LastTerminationReason = LoopTerminationMode.Normal).Value;
    }

    /// <summary>
    /// This method tests if any breakpoint is reached during the execution of the machine frame to suspend the loop.
    /// </summary>
    private void CheckBreakpoints()
    {
        // TODO: Implement this method
    }

    /// <summary>
    /// This method tests if the CPU reached the specified termination point.
    /// </summary>
    /// <returns>
    /// True, if the execution has reached the termination point; otherwise, false.
    /// </returns>
    /// <remarks>
    /// By default, this method checks if the PC equals the execution context's TerminationPoint value. 
    /// </remarks>
    protected virtual bool TestTerminationPoint() 
        => ExecutionContext.LoopTerminationMode == LoopTerminationMode.UntilExecutionPoint && 
           Cpu.Regs.PC == ExecutionContext.TerminationPoint;

    /// <summary>
    /// The machine's execution loop calls this method to check if it can change the clock multiplier.
    /// </summary>
    /// <returns>
    /// True, if the clock multiplier can be changed; otherwise, false.
    /// </returns>
    protected virtual bool AllowCpuClockChange()
    {
        return true;
    }

    /// <summary>
    /// The machine's execution loop calls this method when it is about to initialize a new frame.
    /// </summary>
    /// <param name="clockMultiplierChanged">
    /// Indicates if the clock multiplier has been changed since the execution of the previous frame.
    /// </param>
    protected virtual void OnInitNewFrame(bool clockMultiplierChanged)
    {
        // --- Override this method in derived classes.
    }

    /// <summary>
    /// Tests if the machine should raise a Z80 maskable interrupt
    /// </summary>
    /// <returns>
    /// True, if the INT signal should be active; otherwise, false.
    /// </returns>
    protected abstract bool ShouldRaiseInterrupt();

    /// <summary>
    /// The machine frame loop invokes this method after executing a CPU instruction.
    /// </summary>
    protected virtual void AfterInstructionExecuted()
    {
        // --- Override this method in derived classes.
    }
}
