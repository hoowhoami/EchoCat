using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EchoCat.AI;
using EchoCat.Core;
using EchoCat.Desktop.Services;
using EchoCat.Desktop.Skins;

namespace EchoCat.Desktop.ViewModels;

public sealed partial class PetViewModel : ObservableObject
{
    private readonly IAiCompanionService _ai;
    private readonly ILocalSettingsService _settings;
    private readonly PetSkinCatalog _skins;
    private readonly PetStateMachine _state;
    private readonly DispatcherTimer _timer;
    private int _bubbleTicksRemaining = 50;
    private int _idleMotionTicks;

    [ObservableProperty]
    private PetMood mood;

    [ObservableProperty]
    private int affinity;

    [ObservableProperty]
    private int energy;

    [ObservableProperty]
    private string action = string.Empty;

    [ObservableProperty]
    private string bubbleText = string.Empty;

    [ObservableProperty]
    private bool isBubbleVisible = true;

    [ObservableProperty]
    private bool isChatOpen;

    [ObservableProperty]
    private string promptText = string.Empty;

    [ObservableProperty]
    private double breathingPhase;

    [ObservableProperty]
    private double lookX;

    [ObservableProperty]
    private double lookY;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private PetSkinPalette skinPalette = PetSkinPalette.Default(PetMood.Calm);

    [ObservableProperty]
    private string activeSkinName = string.Empty;

    [ObservableProperty]
    private string activeSkinId = string.Empty;

    [ObservableProperty]
    private IPetSkin activeSkin = new SoftVectorCatSkin();

    public IReadOnlyList<PetSkinOption> SkinOptions { get; }

    public PetViewModel(IAiCompanionService ai, ILocalSettingsService settings, PetSkinCatalog skins, PetStateMachine state)
    {
        _ai = ai;
        _settings = settings;
        _skins = skins;
        _state = state;
        SkinOptions = _skins.Skins
            .Select(skin => new PetSkinOption(skin.Id, skin.DisplayName))
            .ToList();
        ActiveSkin = _skins.ActivePetSkin;
        ActiveSkinName = ActiveSkin.DisplayName;
        ActiveSkinId = ActiveSkin.Id;
        Apply(_state.Snapshot, bubbleTicks: 50);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(120)
        };
        _timer.Tick += (_, _) => Animate();
        _timer.Start();
        _ = RestorePreferencesAsync();
    }

    [RelayCommand]
    private void SwitchSkin(string skinId)
    {
        ActiveSkin = _skins.GetSkin(skinId);
        ActiveSkinId = ActiveSkin.Id;
        ActiveSkinName = ActiveSkin.DisplayName;
        SkinPalette = ActiveSkin.CreatePalette(Mood);
        ShowBubble($"换好啦：{ActiveSkinName}", bubbleTicks: 24);
        _ = _settings.SaveActiveSkinAsync(ActiveSkinId);
    }

    [RelayCommand]
    private void Pet()
    {
        Apply(_state.Accept(PetSignal.Petted), bubbleTicks: 0);
        HideBubble();
    }

    [RelayCommand]
    private void OpenChat()
    {
        IsChatOpen = true;
        HideBubble();
    }

    [RelayCommand]
    private void CloseChat()
    {
        IsChatOpen = false;
        PromptText = string.Empty;
        HideBubble();
    }

    [RelayCommand]
    private void ToggleChat()
    {
        if (IsChatOpen)
        {
            CloseChat();
        }
        else
        {
            OpenChat();
        }
    }

    [RelayCommand]
    private void Dragged()
    {
        IsChatOpen = false;
        Apply(_state.Accept(PetSignal.Dragged), bubbleTicks: 0);
        HideBubble();
    }

    [RelayCommand]
    private void Dragging()
    {
        IsChatOpen = false;
        IsBubbleVisible = false;
        Action = "sway";
        Mood = PetMood.Curious;
        SkinPalette = ActiveSkin.CreatePalette(Mood);
    }

    public void LookAt(double x, double y)
    {
        LookX = Math.Clamp(x, -1, 1);
        LookY = Math.Clamp(y, -1, 1);
    }

    public void ResetLook()
    {
        LookX = 0;
        LookY = 0;
    }

    public void SetEdgeProximity(bool isNearEdge)
    {
        if (!isNearEdge || IsChatOpen || Action is "sway" or "purr" or "listen" or "reply")
        {
            return;
        }

        Mood = PetMood.Curious;
        Action = "peek";
        SkinPalette = ActiveSkin.CreatePalette(Mood);
    }

    [RelayCommand]
    private async Task AskAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var message = PromptText.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            ShowBubble("先给我一句话，我再动脑袋。", bubbleTicks: 34);
            return;
        }

        PromptText = string.Empty;
        Apply(_state.Accept(PetSignal.Asked, message), bubbleTicks: 18);

        try
        {
            IsBusy = true;
            var response = await _ai.ReplyAsync(message);
            Apply(_state.Accept(PetSignal.Answered, response.Text), bubbleTicks: 90);
            IsChatOpen = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Animate()
    {
        BreathingPhase = (BreathingPhase + 0.035) % 1.0;
        _idleMotionTicks++;

        if (!IsChatOpen && _bubbleTicksRemaining > 0)
        {
            _bubbleTicksRemaining--;

            if (_bubbleTicksRemaining == 0)
            {
                HideBubble();
            }
        }

        if (BreathingPhase < 0.035)
        {
            var before = BubbleText;
            Apply(_state.Accept(PetSignal.IdleTick), bubbleTicks: 0);

            if (Action == "nap" && BubbleText != before && _idleMotionTicks > 180)
            {
                IsBubbleVisible = true;
                _bubbleTicksRemaining = 28;
                _idleMotionTicks = 0;
            }
        }
    }

    private void Apply(PetSnapshot snapshot, int bubbleTicks)
    {
        Mood = snapshot.Mood;
        Affinity = snapshot.Affinity;
        Energy = snapshot.Energy;
        Action = snapshot.Action;
        BubbleText = snapshot.BubbleText;
        SkinPalette = ActiveSkin.CreatePalette(snapshot.Mood);

        if (bubbleTicks > 0 || IsChatOpen)
        {
            IsBubbleVisible = true;
            _bubbleTicksRemaining = bubbleTicks;
        }
    }

    private void ShowBubble(string text, int bubbleTicks)
    {
        BubbleText = text;
        IsBubbleVisible = true;
        _bubbleTicksRemaining = bubbleTicks;
    }

    private void HideBubble()
    {
        if (!IsChatOpen)
        {
            IsBubbleVisible = false;
        }
    }

    private async Task RestorePreferencesAsync()
    {
        var settings = await _settings.LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.ActiveSkinId))
        {
            ActiveSkin = _skins.GetSkin(settings.ActiveSkinId);
            ActiveSkinId = ActiveSkin.Id;
            ActiveSkinName = ActiveSkin.DisplayName;
            SkinPalette = ActiveSkin.CreatePalette(Mood);
        }

        if (!settings.ShowWelcomeBubble)
        {
            HideBubble();
        }
    }
}
