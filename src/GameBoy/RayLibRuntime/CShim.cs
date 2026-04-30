using System.Buffers;
using System.Runtime.InteropServices;
using RayLibNet.Interop;

namespace GameBoy.RayLibRuntime;

internal static class CShim
{
    private unsafe delegate int FormatVaListDelegate(sbyte* buffer, nuint size, byte* format, void* args);

    private static unsafe FormatVaListDelegate vsnprintf
    {
        get
        {
            return field ??= OperatingSystem.IsWindows() ? Msvcrt_vsnprintf : LibC_vsnprintf;

            [DllImport("msvcrt", CallingConvention = CallingConvention.Cdecl, EntryPoint = "_vsnprintf")]
            static extern int Msvcrt_vsnprintf(sbyte* buffer, nuint size, byte* format, void* args);

            [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "vsnprintf")]
            static extern int LibC_vsnprintf(sbyte* buffer, nuint size, byte* format, void* args);
        }
    }

    public static unsafe string FormatVaList(byte* format, void* args)
    {
        sbyte[]? buffer = null;

        try
        {
            buffer = ArrayPool<sbyte>.Shared.Rent(4096);
            fixed (sbyte* pBuffer = buffer)
            {
                vsnprintf.Invoke(pBuffer, (nuint)buffer.Length, format, args);

                var message = new string(pBuffer);

                return message;
            }
        }
        finally
        {
            if (buffer is not null)
                ArrayPool<sbyte>.Shared.Return(buffer);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static unsafe void TraceLogCallback(int logLevel, byte* format, void* args)
    {
        var message = FormatVaList(format, args);
        switch ((TraceLogLevel)logLevel)
        {
            case TraceLogLevel.LOG_ALL: break;
            case TraceLogLevel.LOG_TRACE: break;
            case TraceLogLevel.LOG_DEBUG: break;
            case TraceLogLevel.LOG_INFO: break;
            case TraceLogLevel.LOG_WARNING: break;
            case TraceLogLevel.LOG_ERROR:
            case TraceLogLevel.LOG_FATAL:
                break;
            case TraceLogLevel.LOG_NONE: break;
            default: throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
}
