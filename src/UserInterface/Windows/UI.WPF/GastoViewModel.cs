using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.WPF
{
    public partial class GastoViewModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private DateOnly data;

        [ObservableProperty]
        private string tipo = string.Empty;

        [ObservableProperty]
        private string pessoa = string.Empty;

        [ObservableProperty]
        private string forma = string.Empty;

        [ObservableProperty]
        private string descricao = string.Empty;

        [ObservableProperty]
        private float valor;
    }
}
