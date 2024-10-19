namespace game_client;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        NavigationPage.SetHasBackButton(this, false);
        NavigationPage.SetHasNavigationBar(this, false);
	}

    private void OnSetting1Clicked(object sender, EventArgs e)
    {
        
    }

    private void OnSetting2Clicked(object sender, EventArgs e)
    {
        // Navigate to Settings page or perform some action
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

