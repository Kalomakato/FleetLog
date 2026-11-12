using FleetLog.Services;
using Xunit;

namespace FleetLog.Tests;

public class MileageRulesTests
{
    [Fact]
    public void IsReadingValid_AcceptsCurrentMileage()
    {
        Assert.True(MileageRules.IsReadingValid(
            currentMileageKm: 86_200, newMileageKm: 86_200));
    }

    [Fact]
    public void IsReadingValid_RejectsOneKilometreBelowCurrentMileage()
    {
        Assert.False(MileageRules.IsReadingValid(
            currentMileageKm: 86_200, newMileageKm: 86_199));
    }
}