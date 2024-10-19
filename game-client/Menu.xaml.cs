namespace game_client;

public partial class Menu : ContentPage
{
	public Menu()
	{
		InitializeComponent();
	}

    private async void OnPlayClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GameWorldPage());
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}

