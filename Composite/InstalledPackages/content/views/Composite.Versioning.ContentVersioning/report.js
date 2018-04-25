/**
 * @class
 */
var VersioningReport = new function () {

	this.Show = function (providerName, serializedEntityToken, serializedActionToken, piggybag, piggybagHash) {
		var versioning = "Versioning";
		Application.lock(versioning);
		setTimeout(function () {

			TreeService.ExecuteSingleElementAction(
				GetClientElement(serializedEntityToken, piggybag, piggybagHash),
				serializedActionToken,
				Application.CONSOLE_ID
			);
			MessageQueue.update();
			Application.unlock(versioning);
		}, 0);
	}

	this.ShowFile = function (activityId) {
		window.location = Resolver.resolve("${root}/InstalledPackages/services/Composite.Versioning.ContentVersioning/ViewFile.ashx?activityId=" + activityId);
	}

}

function GetClientElement(entityToken, piggybag, piggybagHash) {
	this.ProviderName = 'PageElementProvider';
	this.EntityToken = entityToken;
    this.Piggybag = piggybag;
    this.PiggybagHash = piggybagHash;
	this.HasChildren = false;
	this.IsDisabled = false;
	this.DetailedDropSupported = false;
	this.ContainsTaggedActions = false;
	this.TreeLockEnabled = false;
	return this;
}
