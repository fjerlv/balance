using Xunit;

namespace Balance.Tests;

public class HealthCalculatorTests
{
    [Fact]
    public void CalculateHealth_EqualKillsAndDeaths_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 5, deaths: 5);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateHealth_MoreKillsThanDeaths_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 10, deaths: 3);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateHealth_OneMoreDeathThanKills_Returns120()
    {
        var result = HealthCalculator.CalculateHealth(kills: 5, deaths: 6);

        Assert.Equal(120, result);
    }

    [Fact]
    public void CalculateHealth_ThreeMoreDeathsThanKills_Returns160()
    {
        var result = HealthCalculator.CalculateHealth(kills: 2, deaths: 5);

        Assert.Equal(160, result);
    }

    [Fact]
    public void CalculateHealth_ZeroKillsFiveDeaths_Returns200()
    {
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 5);

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
    public void CalculateHealth_VariousInputs_ReturnsExpectedHealth(int kills, int deaths, int expectedHealth)
    {
        var result = HealthCalculator.CalculateHealth(kills, deaths);

        Assert.Equal(expectedHealth, result);
    }

    [Fact]
    public void CalculateHealth_LargeDeathDeficit_ReturnsHighHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 0, deaths: 20);

        Assert.Equal(500, result);
    }

    [Fact]
    public void CalculateHealth_LargeKillSurplus_ReturnsBaseHealth()
    {
        var result = HealthCalculator.CalculateHealth(kills: 50, deaths: 0);

        Assert.Equal(100, result);
    }
}
