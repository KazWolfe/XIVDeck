using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class NetworkUtil {
    /// <summary>
    /// Check for IPv6 support on the current host by checking if the host itself supports IPv6 and has a defined
    /// loopback interface address.
    /// </summary>
    /// <remarks>
    /// This method relies on some arcane Windows magic conveniently exposed in a very nice interface. If no IPv6
    /// route is available, IPv6LoopbackInterfaceIndex will throw an exception which we can deal with.
    ///
    /// In Windows environments, this will almost <i>always</i> be <c>true</c>, but some Linux users have disabled
    /// IPv6 with such prejudice that it doesn't even exist on loopback interfaces.
    /// </remarks>
    /// <returns>Returns <c>true</c> if a local IPv6 interface should exist, false otherwise.</returns>
    public static bool HostSupportsLocalIPv6() {
        if (!Socket.OSSupportsIPv6) return false;
        
        try {
            _ = NetworkInterface.IPv6LoopbackInterfaceIndex;
            return true;
        } catch (NetworkInformationException) {
            return false;
        }
    }
    
    public static bool HostSupportsLocalIPv4() {
        if (!Socket.OSSupportsIPv4) return false;
        
        try {
            _ = NetworkInterface.LoopbackInterfaceIndex;
            return true;
        } catch (NetworkInformationException) {
            return false;
        }
    }
}