using Android.Content;
using AndroidX.Biometric;
using Microsoft.Maui.Controls;
using AssetManagement.Services;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(AssetManagement.Platforms.Android.BiometricService))]
namespace AssetManagement.Platforms.Android
{
    public class BiometricService : IBiometricService
    {
        public bool IsBiometricEnrolled()
        {
            var biometricManager = BiometricManager.From(global::Android.App.Application.Context);
            var result = biometricManager.CanAuthenticate(BiometricManager.Authenticators.BiometricStrong | BiometricManager.Authenticators.DeviceCredential);

            return result == BiometricManager.BiometricSuccess;
        }
    }
}