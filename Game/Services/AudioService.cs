using NAudio.Wave;
using Serilog;

namespace Game.Services;

/// <summary>
/// Service for playing audio files (music and sound effects).
/// </summary>
public class AudioService : IDisposable
{
    private WaveOutEvent? _musicPlayer;
    private AudioFileReader? _currentMusic;
    private bool _disposed;

    /// <summary>
    /// Play background music in a loop.
    /// </summary>
    public void PlayMusic(string filePath)
    {
        try
        {
            StopMusic();

            _currentMusic = new AudioFileReader(filePath);
            _musicPlayer = new WaveOutEvent();
            _musicPlayer.Init(_currentMusic);
            
            // Loop the music
            _musicPlayer.PlaybackStopped += (sender, args) =>
            {
                if (!_disposed && _currentMusic != null)
                {
                    _currentMusic.Position = 0;
                    _musicPlayer?.Play();
                }
            };

            _musicPlayer.Play();
            Log.Information("Playing background music: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to play music: {FilePath}", filePath);
        }
    }

    /// <summary>
    /// Stop the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        _musicPlayer?.Stop();
        _musicPlayer?.Dispose();
        _currentMusic?.Dispose();
        _musicPlayer = null;
        _currentMusic = null;
    }

    /// <summary>
    /// Play a sound effect once.
    /// </summary>
    public void PlaySoundEffect(string filePath)
    {
        Task.Run(() =>
        {
            try
            {
                using var audioFile = new AudioFileReader(filePath);
                using var outputDevice = new WaveOutEvent();
                outputDevice.Init(audioFile);
                outputDevice.Play();

                // Wait for playback to finish
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(100);
                }

                Log.Debug("Played sound effect: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to play sound effect: {FilePath}", filePath);
            }
        });
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        StopMusic();
        GC.SuppressFinalize(this);
    }
}
