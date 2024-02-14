using CommunityToolkit.Mvvm.Messaging.Messages;
using NAudio.Wave;

namespace MemeBox.Messages
{
    internal class PlaybackStateChangedMessage : ValueChangedMessage<PlaybackState>
    {
        public PlaybackStateChangedMessage(PlaybackState value) : base(value) { }
    }
}
