using Godot;
namespace Options
{
	[GlobalClass, GodotClassName("LanguageSelector"), Tool]
	public partial class LanguageSelector : OptionButtonController
	{
		public override void CreateOptions()
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

				if (currentLocale == l)
					Select(i);
			}
		}

		public override void OnItemSelected(Variant data)
		{
			base.OnItemSelected(data);

			TranslationServer.SetLocale((string)data);
		}
	}
}