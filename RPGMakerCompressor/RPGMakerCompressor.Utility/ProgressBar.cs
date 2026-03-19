using System;
using System.Text;
using System.Threading;

// This is a modified version of code taken from (https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54)
public class ProgressBar : IDisposable, IProgress<double>
{
    private const int blockCount = 20;
    private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
    private readonly TimeSpan progressInterval = TimeSpan.FromSeconds(1.0 / 50);
    private const string animation = @"|/-\";

    private readonly Timer animationtimer;
    private readonly Timer timer;

    private double currentProgress = 0;
    private string currentText = string.Empty;
    private bool disposed = false;
    private int animationIndex = 0;
    private ConsoleColor timerColor = ConsoleColor.Gray;

    private char animationState = '|';

    public ProgressBar()
    {
        timer = new Timer(TimerHandler);
        animationtimer = new Timer(AnimationTimerHandler);

        // A progress bar is only for temporary display in a console window.
        // If the console output is redirected to a file, draw nothing.
        // Otherwise, we'll end up with a lot of garbage in the target file.
        if (!Console.IsOutputRedirected)
        {
            ResetTimer();
            ResetAnimationTimer();
        }
    }
    
    public void SetTimerColor(ConsoleColor color)
    {
        timerColor = color;
    }

    public void Report(double value)
    {
        // Make sure value is in [0..1] range
        value = Math.Max(0, Math.Min(1, value));
        Interlocked.Exchange(ref currentProgress, value);
    }

    private void AnimationTimerHandler(object state)
    {
        lock (animationtimer)
        {
            if (disposed) return;

            animationState = animation[animationIndex++ % animation.Length];
            ResetAnimationTimer();
        }
    }

    private void TimerHandler(object state)
    {
        lock (timer)
        {
            if (disposed) return;

            int progressBlockCount = (int)(currentProgress * blockCount);
            int percent = (int)(currentProgress * 100);
            string text = string.Format("[{0}{1}] {2,3}% {3}",
                new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                percent,
                animationState);
            UpdateText(text);

            ResetTimer();
        }
    }

    public void UpdateText(string text)
    {
        // Get length of common portion
        int commonPrefixLength = 0;
        int commonLength = Math.Min(currentText.Length, text.Length);
        while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
        {
            commonPrefixLength++;
        }

        // Backtrack to the first differing character
        StringBuilder outputBuilder = new StringBuilder();
        outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

        // Output new suffix
        outputBuilder.Append(text.Substring(commonPrefixLength));

        // If the new text is shorter than the old one: delete overlapping characters
        int overlapCount = currentText.Length - text.Length;
        if (overlapCount > 0)
        {
            outputBuilder.Append(' ', overlapCount);
            outputBuilder.Append('\b', overlapCount);
        }

        Console.ForegroundColor = timerColor;
        Console.Write(outputBuilder);
        Console.ResetColor();

        currentText = text;
    }

    private void ResetTimer()
    {
        timer.Change(progressInterval, TimeSpan.FromMilliseconds(-1));
    }

    private void ResetAnimationTimer()
    {
        animationtimer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
    }

    public void Dispose()
    {
        if (disposed) return;

        lock (timer)
        {
            disposed = true;
            UpdateText(string.Empty);
        }
    }

}