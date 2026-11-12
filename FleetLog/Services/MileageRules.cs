namespace FleetLog.Services;

public static class MileageRules
{
    public static bool IsReadingValid(int currentMileageKm, int newMileageKm)
    {
        return newMileageKm >= currentMileageKm;
    }
}