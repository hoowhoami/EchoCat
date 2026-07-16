namespace EchoCat.Core;

public sealed class PetStateMachine
{
    private int _idleTicks;

    public PetMood Mood { get; private set; } = PetMood.Calm;

    public int Affinity { get; private set; } = 10;

    public int Energy { get; private set; } = 80;

    public string Action { get; private set; } = "loaf";

    public string BubbleText { get; private set; } = "我在。轻轻点我一下吧。";

    public PetSnapshot Snapshot => new(Mood, Affinity, Energy, Action, BubbleText);

    public PetSnapshot Accept(PetSignal signal, string? text = null)
    {
        switch (signal)
        {
            case PetSignal.IdleTick:
                TickIdle();
                break;
            case PetSignal.Petted:
                _idleTicks = 0;
                Affinity = Math.Min(100, Affinity + 3);
                Energy = Math.Min(100, Energy + 1);
                Mood = PetMood.Happy;
                Action = "purr";
                BubbleText = "呼噜。今天也辛苦啦。";
                break;
            case PetSignal.Asked:
                _idleTicks = 0;
                Energy = Math.Max(0, Energy - 2);
                Mood = PetMood.Thinking;
                Action = "listen";
                BubbleText = string.IsNullOrWhiteSpace(text) ? "嗯？" : "我想想。";
                break;
            case PetSignal.Answered:
                Mood = PetMood.Curious;
                Action = "reply";
                BubbleText = text ?? "好。";
                break;
            case PetSignal.Dragged:
                _idleTicks = 0;
                Mood = PetMood.Curious;
                Action = "sway";
                BubbleText = "搬家成功。这个角落还不错。";
                break;
            case PetSignal.LongIdle:
                Mood = PetMood.Sleepy;
                Action = "nap";
                BubbleText = "我先眯一小会儿。";
                break;
        }

        return Snapshot;
    }

    private void TickIdle()
    {
        _idleTicks++;
        Energy = Math.Max(0, Energy - 1);

        if (_idleTicks > 24 || Energy < 20)
        {
            Accept(PetSignal.LongIdle);
            return;
        }

        if (_idleTicks % 16 == 0)
        {
            Mood = PetMood.Calm;
            Action = "stretch";
            BubbleText = "伸个懒腰，再陪你一会儿。";
        }
        else if (_idleTicks % 8 == 0)
        {
            Mood = PetMood.Calm;
            Action = "blink";
            BubbleText = "我会安静陪你。";
        }
    }
}
