namespace EchoCat.Core;

public sealed record PetSnapshot(
    PetMood Mood,
    int Affinity,
    int Energy,
    string Action,
    string BubbleText);
