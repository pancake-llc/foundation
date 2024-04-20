using Heart;
using Pancake.BakingSheet;
using Pancake.Localization;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        var sheetContainer = new SheetContainer();
        var googleConverter = new GoogleSheetConverter(LocaleSettings.SpreadsheetKey, LocaleSettings.ServiceAccountCredential);

        await sheetContainer.Bake(googleConverter);

        var jsonConverter = new CsvSheetConverter("Assets/Demo");
        Debug.Log("A");

        await sheetContainer.Store(jsonConverter);
    }

    // Update is called once per frame
    void Update() { }
}