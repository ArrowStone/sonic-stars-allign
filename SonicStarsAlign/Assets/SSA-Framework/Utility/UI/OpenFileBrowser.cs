using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleFileBrowser;
using System.Collections;
using System.IO;

public class OpenFileBrowser : MonoBehaviour
{
    [SerializeField] TMP_InputField input;

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
           StartCoroutine(ShowLoadDialogCoroutine());
    }


    // Credit: UNitySimpleFileBrowser GitHub
    IEnumerator ShowLoadDialogCoroutine()
	{
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Object JSON files", ".json"));
        FileBrowser.SetDefaultFilter( ".json" );

		// Show a load file dialog and wait for a response from user
		// Load file/folder: file, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load" );

		// Dialog is closed
		// Print whether the user has selected some files or cancelled the operation (FileBrowser.Success)
		Debug.Log( FileBrowser.Success );

		if( FileBrowser.Success )
			OnFilesSelected( FileBrowser.Result ); // FileBrowser.Result is null, if FileBrowser.Success is false
	}
	
	void OnFilesSelected( string[] filePaths )
	{
		input.text = filePaths[0];
	}
}