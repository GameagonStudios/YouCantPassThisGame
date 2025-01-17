using Godot;

[GlobalClass, Tool]
public partial class LanguageSelector : OptionButton
{
	StringName key = "LANGUAGE";

	public override void _EnterTree()
	{
		ItemSelected += OnItemSelected;
 
		CreateOptions();
	}

	public override void _ExitTree()
	{
		ItemSelected -= OnItemSelected;
	}

	public void CreateOptions()
	{
		string[] locales = TranslationServer.GetLoadedLocales();

		string currentLocale;

		currentLocale = OptionsSavesHandler.Current?.GetValue(key)?.ToString() ?? OS.GetLocale();

		TranslationServer.SetLocale(currentLocale);

		Clear();

		for (int i = 0; i < locales.Length; i++)
		{
			string l = locales[i];

			AddItem(TranslationServer.GetTranslationObject(l).GetMessage("LANGUAGE_NAME"), i);
			SetItemMetadata(i, l);

			if(currentLocale == l)
				Select(i);
		}
	}

	public void OnItemSelected(long index)
	{
		var l = (string)GetSelectedMetadata();
		TranslationServer.SetLocale(l);
		OptionsSavesHandler.Current.SetValue(key, l);
	}
}
