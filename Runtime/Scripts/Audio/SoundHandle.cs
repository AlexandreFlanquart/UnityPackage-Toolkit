namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Reference to one sound started by <see cref="SFXManager"/>. Stays safe after the sound ends:
    /// once its pooled source is reused by another sound, the handle reports not playing and <see cref="Stop"/> does nothing.
    /// The default value means "nothing was played" (request rejected).
    /// </summary>
    public readonly struct SoundHandle
    {
        private readonly SFXManager _manager;
        private readonly int _sourceIndex;
        private readonly int _generation;

        internal SoundHandle(SFXManager manager, int sourceIndex, int generation)
        {
            _manager = manager;
            _sourceIndex = sourceIndex;
            _generation = generation;
        }

        internal int SourceIndex => _sourceIndex;
        internal int Generation => _generation;

        /// <summary>True while this exact sound is still playing.</summary>
        public bool IsPlaying => _manager != null && _manager.IsPlaying(this);

        /// <summary>Stops this sound if it is still playing.</summary>
        public void Stop()
        {
            if (_manager != null) _manager.Stop(this);
        }
    }
}
