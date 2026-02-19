class ModConfig
{
    public bool EnableBetterFishingMod { get; set; } = true;
    public bool UnbreakableTackle { get; set; } = true;
    public bool SkipMiniGame { get; set; } = true;
    public bool SkipMiniGameOfLegendaryFish { get; set; } = false;
    public bool JunkDoesNotReduceBait { get; set; } = true;
    public bool AutoReelInFish { get; set; } = false;
    public bool AutoObtainTreasureChest { get; set; } = true;
    public bool AutoGrabTreasureLoot { get; set; } = false;    
    public bool AlwaysCastMaxDistance { get; set; } = false;

    public bool AlwaysPerfect { get; set; } = false;
    public int ReactionTime = 300;
    public int[] MotionTyprFactor = { 5, 20, -5, 10, 10 };
    public bool IfPerfect(float difficulty, int bobberBarHeight, int motionType)
    {
        return new Random().NextDouble() <= (Math.Pow(0.99, 4.21 * (difficulty + MotionTyprFactor[motionType] - ((double)bobberBarHeight - 96) * 0.1875)) - 0.01);
    }
}
