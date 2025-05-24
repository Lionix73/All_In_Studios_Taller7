using Unity.Netcode;

public struct MultiEnemyAgentConfig : INetworkSerializable
{
    public float acceleration;
    public float angularSpeed;
    public int areaMask;
    public int avoidancePriority;
    public float baseOffset;
    public float height;
    public int obstacleAvoidanceType; // Usamos int para el enum
    public float radius;
    public float speed;
    public float stoppingDistance;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref acceleration);
        serializer.SerializeValue(ref angularSpeed);
        serializer.SerializeValue(ref areaMask);
        serializer.SerializeValue(ref avoidancePriority);
        serializer.SerializeValue(ref baseOffset);
        serializer.SerializeValue(ref height);
        serializer.SerializeValue(ref obstacleAvoidanceType);
        serializer.SerializeValue(ref radius);
        serializer.SerializeValue(ref speed);
        serializer.SerializeValue(ref stoppingDistance);
    }
}