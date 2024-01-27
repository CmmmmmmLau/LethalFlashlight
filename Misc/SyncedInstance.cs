using System;
using System.IO;
using System.Runtime.Serialization;
using Unity.Netcode;
using Unity.Collections;

namespace LethalFlashlight.Misc;

[Serializable]
public class SyncedInstance<T> {
    internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
    internal static bool IsClient => NetworkManager.Singleton.IsClient;
    internal static bool IsHost => NetworkManager.Singleton.IsHost;

    [NonSerialized] static readonly DataContractSerializer Serializer = new(typeof(T));
    [NonSerialized] protected static int INT_SIZE = 4;
    [NonSerialized] static int MAX_BUFFER_SIZE;

    public static T Default { get; private set; }
    public static T Instance { get; private set; }

    internal static bool Synced;

    protected void InitInstance(T instance, int maxSize = 1300) {
        Default = instance;
        Instance = instance;
        
        // Ensures the size of an integer is correct for the current system.
        INT_SIZE = sizeof(int);

        // Limit to the size of a single packet upon which fragmenting is required.
        if (maxSize < 1300) {
            MAX_BUFFER_SIZE = maxSize;
        }
    }

    internal static void SyncInstance(byte[] data) {
        Instance = DeserializeFromBytes(data);
        Synced = true;
    }

    internal static void RevertSync() {
        Instance = Default;
        Synced = false;
    }

    public static byte[] SerializeToBytes(T val) {
        using MemoryStream stream = new();

        try {
            Serializer.WriteObject(stream, val);
            return stream.ToArray();
        }
        catch (Exception e) {
            Plugin.mls.LogError($"Error serializing instance: {e}");
            return null;
        }
    }

    public static T DeserializeFromBytes(byte[] data) {
        using MemoryStream stream = new(data);

        try {
            return (T) Serializer.ReadObject(stream);
        } catch (Exception e) {
            Plugin.mls.LogError($"Error deserializing instance: {e}");
            return default;
        }
    }

    internal static void SendMessage(string label, ulong clientId, FastBufferWriter stream) {
        bool fragment = stream.Capacity > MAX_BUFFER_SIZE;
        if (fragment) Plugin.mls.LogDebug(
            $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
            "Config instance will be sent in fragments to avoid overflowing the buffer."
        );

        NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;
        MessageManager.SendNamedMessage(label, clientId, stream, delivery);
    }
    
    public static void RequestSync() {
        if (!IsClient) return;

        using FastBufferWriter stream = new(INT_SIZE, Allocator.Temp);
        SendMessage("ModName_OnRequestConfigSync", 0uL, stream);
    }
    
    public static void OnRequestSync(ulong clientId, FastBufferReader _) {
        if (!IsHost) return;

        Plugin.mls.LogDebug($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + INT_SIZE, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            SendMessage("ModName_OnReceiveConfigSync", clientId, stream);
        } catch(Exception e) {
            Plugin.mls.LogDebug($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }
    
    public static void OnReceiveSync(ulong _, FastBufferReader reader) {
        if (!reader.TryBeginRead(INT_SIZE)) {
            Plugin.mls.LogError("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val)) {
            Plugin.mls.LogError("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);

        Plugin.mls.LogInfo("Successfully synced config with host.");
    }
}