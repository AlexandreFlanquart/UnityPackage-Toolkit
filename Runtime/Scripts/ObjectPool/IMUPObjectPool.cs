namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Non-generic contract implemented by <see cref="MUP_ObjectPool{T}"/>, letting
    /// <see cref="MUP_ObjectPoolLocator"/> manage pools of different pooled types uniformly (e.g. bulk <see cref="Clear"/>).
    /// </summary>
    public interface IMUPObjectPool
    {
        /// <summary>Destroys every idle pooled instance and empties the queue.</summary>
        void Clear();
    }
}
