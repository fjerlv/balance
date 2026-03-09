using Xunit;

namespace Balance.Tests;

public class HealthCalculatorTests
{
    // Equal teams (no team bonus)
    [Fact]
    public void CalculateHealth_EqualKillsAndDeaths_EqualTeams_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 5, deaths: 5, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateHealth_MoreKillsThanDeaths_EqualTeams_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 10, deaths: 3, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateHealth_OneMoreDeathThanKills_EqualTeams_Returns120()
    {
        var result = HealthCalculator.CalculateHealth(kills: 5, deaths: 6, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(120, result);
    }

    [Fact]
    public void CalculateHealth_ThreeMoreDeathsThanKills_EqualTeams_Returns160()
    {
        var result = HealthCalculator.CalculateHealth(kills: 2, deaths: 5, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(160, result);
    }

    [Fact]
    public void CalculateHealth_ZeroKillsFiveDeaths_EqualTeams_Returns200()
    {
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 5, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(200, result);
    }

    [Theory]
    [InlineData(0, 0, 100)]
    [InlineData(1, 0, 100)]
    [InlineData(5, 3, 100)]
    [InlineData(0, 1, 120)]
    [InlineData(0, 2, 140)]
    [InlineData(3, 5, 140)]
    [InlineData(0, 10, 300)]
    public void CalculateHealth_EqualTeams_VariousInputs_ReturnsExpectedHealth(int kills, int deaths, int expectedHealth)
    {
        var result = HealthCalculator.CalculateHealth(kills, deaths, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(expectedHealth, result);
    }

    [Fact]
    public void CalculateHealth_LargeDeathDeficit_EqualTeams_ReturnsHighHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 20, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(500, result);
    }

    [Fact]
    public void CalculateHealth_LargeKillSurplus_EqualTeams_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 50, deaths: 0, myTeamCount: 5, opponentCount: 5);

        Assert.Equal(100, result);
    }

    // Team size bonus tests
    [Fact]
    public void CalculateHealth_OneVsTwo_Returns200()
    {
        // 1v2: 1 missing → 100/1 = +100 HP
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 0, myTeamCount: 1, opponentCount: 2);

        Assert.Equal(200, result);
    }

    [Fact]
    public void CalculateHealth_TwoVsThree_Returns150()
    {
        // 2v3: 1 missing → 100/2 = +50 HP each
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 0, myTeamCount: 2, opponentCount: 3);

        Assert.Equal(150, result);
    }

    [Fact]
    public void CalculateHealth_ThreeVsFive_Returns166()
    {
        // 3v5: 2 missing → 200/3 = +66 HP each
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 0, myTeamCount: 3, opponentCount: 5);

        Assert.Equal(166, result);
    }

    [Fact]
    public void CalculateHealth_LargerTeam_NoBonus()
    {
        // 5v3: larger team gets no bonus
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 0, myTeamCount: 5, opponentCount: 3);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateHealth_TeamBonusAndPerformanceBonus_Stacks()
    {
        // 1v2: +100 team bonus; 2 kills 4 deaths: +40 performance bonus → 240
        var result = HealthCalculator.CalculateHealth(kills: 2, deaths: 4, myTeamCount: 1, opponentCount: 2);

        Assert.Equal(240, result);
    }
}
