using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace UI.WPF
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly string CSV = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads\\Despesas - 2024 - Gastos.csv";
        private readonly string SECRET = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads\\secret";

        public MainWindowViewModel()
        {
            using var reader = new StreamReader(CSV);
            using var csv = new CsvReader(reader, new CultureInfo("pt-BR"));

            var gastos = new ObservableCollection<GastoViewModel>(csv.GetRecords<GastoViewModel>());

            var now = DateTime.Now;
            var nextMonth = now.AddMonths(1);

            var startCurrentMonth = new DateOnly(now.Year, now.Month, 1);
            var startNextMonth = new DateOnly(nextMonth.Year, nextMonth.Month, 1);

            var gastosFiltrados = gastos.Where(x => x.Data >= startCurrentMonth && x.Data < startNextMonth && x.Forma != "Ticket");

            var gastosTotais = gastosFiltrados.Sum(x => x.Valor);

            var gastosRenato = gastosTotais * 0.6;
            var gastosVicka = gastosTotais * 0.4;

            var renato = gastosFiltrados.Where(x => x.Pessoa == "Renato").Sum(x => x.Valor);
            var vicka = gastosFiltrados.Where(x => x.Pessoa == "Vicka").Sum(x => x.Valor);

            var faltaRenato = gastosRenato - renato;
            var faltaVicka = gastosVicka - vicka;

            if (faltaVicka > faltaRenato)
            {
                Resumo = $"Contas referentes a {now:MMMM} de {now:yyyy}. Vicka deve pagar R$ {faltaVicka:f2}. Gastos Totais: R$ {gastosTotais:f2}, deveria pagar R$ {gastosVicka:f2} e já pagou R$ {vicka:f2}";
            }
            else
            {
                Resumo = $"Contas referentes a {now:MMMM} de {now:yyyy}. Renato deve pagar R$ {faltaRenato:f2}. Gastos Totais: R$ {gastosTotais:f2}, deveria pagar R$ {gastosRenato:f2} e já pagou R$ {renato:f2}";
            }

            Gastos = new ObservableCollection<GastoViewModel>(gastosFiltrados);
        }

        [ObservableProperty]
        private string resumo = string.Empty;

        public ObservableCollection<GastoViewModel> Gastos { get; set; }

        public async Task SendEmail(int month, int year)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Gastos da casa", "gastosdacasavickaetato@gmail.com"));
            message.To.Add(new MailboxAddress("", "renatojsilvas@gmail.com"));
            message.Subject = $"Gastos da casa - {month}/{year}";
            message.Body = new TextPart("plain") { Text = Resumo };

            var secret = File.ReadAllText(SECRET);

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("gastosdacasavickaetato@gmail.com", secret);
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }
    }
}
