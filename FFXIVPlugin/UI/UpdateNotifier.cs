using System;
using System.Threading;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI;

[Service(ServiceFlags.Singleton)]
public class UpdateNotifier(INotificationManager notifications) : IDisposable {
    private readonly Lock _lock = new();

    private IActiveNotification? _forcedUpdateNotification;
    private IActiveNotification? _testingUpdateNotification;

    public void ShowForcedUpdate() {
        lock (this._lock) {
            if (this._forcedUpdateNotification != null) return;

            var versionString = VersionUtils.GetCurrentMajMinBuild();

            this._forcedUpdateNotification = notifications.AddNotification(new Notification {
                Title = UIStrings.UpdateNotifier_ForcedUpdate_Headline,
                Content = $"{UIStrings.UpdateNotifier_ForcedUpdate_ProblemDescription} {UIStrings.Common_SupportInfo}",
                Type = NotificationType.Error,
                InitialDuration = TimeSpan.MaxValue,
            });

            this._forcedUpdateNotification.DrawActions += _ => {
                if (ImGui.Button(UIStrings.Common_OpenGitHubDownloadButton)) {
                    UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{versionString}");
                }

                ImGui.SameLine();
                if (ImGui.Button(UIStrings.UpdateNotifier_ForcedUpdate_SupportButton)) {
                    Util.OpenLink(Constants.GoatPlaceDiscord);
                }
            };
        }
    }

    public void DismissForcedUpdate() {
        lock (this._lock) {
            this._forcedUpdateNotification?.DismissNow();
            this._forcedUpdateNotification = null;
        }
    }

    public void ShowTestingUpdate(Version currentVersion) {
        lock (this._lock) {
            if (this._testingUpdateNotification != null) return;

            this._testingUpdateNotification = notifications.AddNotification(new Notification {
                Title = UIStrings.UpdateNotifier_TestingUpdate_Headline,
                Content = $"{UIStrings.UpdateNotifier_TestingUpdate_MismatchDetectedText} {UIStrings.UpdateNotifier_TestingUpdate_PleaseTestProperly}",
                Type = NotificationType.Warning,
                InitialDuration = TimeSpan.MaxValue,
            });

            this._testingUpdateNotification.DrawActions += notification => {
                if (ImGui.Button(UIStrings.UpdateNotifier_TestingUpdate_IgnoreButton)) {
                    notification.Notification.DismissNow();
                }

                ImGui.SameLine();
                if (ImGui.Button(string.Format(UIStrings.UpdateNotifier_TestingUpdate_DownloadButton, currentVersion))) {
                    UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{currentVersion}");
                }
            };
        }
    }

    public void DismissTestingUpdate() {
        lock (this._lock) {
            this._testingUpdateNotification?.DismissNow();
            this._testingUpdateNotification = null;
        }
    }

    public void Dispose() {
        this.DismissForcedUpdate();
        this.DismissTestingUpdate();

        GC.SuppressFinalize(this);
    }
}
