namespace Kuti.Windows.Preferences
{
    interface IPreferencesPage
    {
        void OnShow() { }

        bool OnApply ();
        void OnCancel() { }
    }
}
