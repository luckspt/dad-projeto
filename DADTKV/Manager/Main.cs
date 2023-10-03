using Parser;
using Parser.Parsers;
using System.Diagnostics;
using System.Threading;

namespace Manager
{
    class ClientStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color SendingRequests = Color.Green;
    }

    class TMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color ExecutingTransaction = Color.LightGreen;
        public static readonly Color CommitingTransaction = Color.DarkGreen;
    }

    class LMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color PaxosProposer = Color.LightGreen;
        public static readonly Color PaxosAcceptor = Color.DarkGreen;
    }

    public partial class Main : Form
    {
        private ConfigParser config = null;
        private List<Pair<ClientConfigLine, Color>> clients = null;
        private List<Pair<ServerConfigLine, Color>> transactionManagers = null;
        private List<Pair<ServerConfigLine, Color>> leaseManagers = null;
        private System.Threading.Timer processTimer = null;

        public Main()
        {
            InitializeComponent();
        }

        private void updateStatuses(object state)
        {
            // Clients
            this.lsvwClients.BeginUpdate();
            if (this.lsvwClients.Items.Count != this.clients.Count)
            {
                this.lsvwClients.Items.Clear();
                this.lsvwClients.Items.AddRange(this.clients.Select(client => new ListViewItem()
                {
                    Text = client.First.ID,
                    ToolTipText = client.First.ScriptPath,
                    BackColor = client.Second,
                }).ToArray());
            }
            else
            {
                for (int i = 0; i < this.lsvwClients.Items.Count; i++)
                {
                    this.lsvwClients.Items[i].BackColor = this.clients[i].Second;
                }
            }
            this.lsvwClients.EndUpdate();

            // TMs
            this.lsvwTMs.BeginUpdate();
            if (this.lsvwTMs.Items.Count != this.transactionManagers.Count)
            {
                this.lsvwTMs.Items.Clear();
                this.lsvwTMs.Items.AddRange(transactionManagers.Select(server => new ListViewItem()
                {
                    Text = server.First.ID,
                    ToolTipText = server.First.Url,
                    BackColor = server.Second,
                }).ToArray());
            }
            else
            {
                for (int i = 0; i < this.lsvwTMs.Items.Count; i++)
                {
                    this.lsvwTMs.Items[i].BackColor = this.transactionManagers[i].Second;
                }
            }
            this.lsvwTMs.EndUpdate();

            // LMs
            this.lsvwLMs.BeginUpdate();
            if (this.lsvwLMs.Items.Count != this.leaseManagers.Count)
            {
                this.lsvwLMs.Items.Clear();
                this.lsvwLMs.Items.AddRange(this.leaseManagers.Select(server => new ListViewItem()
                {
                    Text = server.First.ID,
                    ToolTipText = server.First.Url,
                    BackColor = server.Second,
                }).ToArray());
            }
            else
            {
                for (int i = 0; i < this.lsvwLMs.Items.Count; i++)
                {
                    this.lsvwLMs.Items[i].BackColor = this.leaseManagers[i].Second;
                }
            }
            this.lsvwLMs.EndUpdate();
        }

        private void Entry_Load(object sender, EventArgs e)
        {
            // Set labels
            this.lblClientsLabel.Text = "Gray = Not Started\nYellow = Idle\nGreen = Sending requests";
            this.lblTMsLabel.Text = "Gray = Not Started\nYellow = Idle (waiting for a lease)\nRed = Crashed\nGreen = Executing transaction";
            this.lblLMsLabel.Text = "Gray = Not Started\nYellow = Idle\nRed = Crashed";

            this.fdConfig.Title = "Select the configuration file";
            this.fdConfig.ShowDialog();
        }

        private void fdConfig_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                String filePath = this.fdConfig.FileName;
                this.loadConfig(filePath);
                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void loadConfig(String configFilePath)
        {
            this.config = new ConfigParser(configFilePath);
            this.config.Parse();

            this.clients = this.config.Config[ConfigType.Client]
                .Select(x => new Pair<ClientConfigLine, Color>((ClientConfigLine)x, ClientStatus.NotStarted))
                .ToList();

            this.transactionManagers = this.config.Config[ConfigType.Server]
                .Select(x => new Pair<ServerConfigLine, Color>((ServerConfigLine)x, TMStatus.NotStarted))
                               .Where(server => server.First.Type == ServerType.TransactionManager)
                               .ToList();

            this.leaseManagers = this.config.Config[ConfigType.Server]
                .Select(x => new Pair<ServerConfigLine, Color>((ServerConfigLine)x, LMStatus.NotStarted))
                            .Where(server => server.First.Type == ServerType.LeaseManager)
                            .ToList();

            this.startProcessMonitoring();
            this.startProcesses();
        }

        private void startProcessMonitoring()
        {
            // Update GUI every second
            this.processTimer = new System.Threading.Timer(this.updateStatuses, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            // Start StatusHookService


        }

        private void startProcesses()
        {
            string solutionDirectory = this.getSolutionDirectory();

            // Start LMs
            foreach (Pair<ServerConfigLine, Color> lm in this.leaseManagers)
                Process.Start(solutionDirectory + "/LeaseManager/bin/Debug/net6.0/LeaseManager.exe");

            // Start TMs
            // foreach (Pair<ServerConfigLine, Color> tm in this.transactionManagers)
            // Process.Start(solutionDirectory + "/TransactionManager/bin/Debug/net6.0/TransactionManager.exe");

            // Start Clients
            // foreach (Pair<ClientConfigLine, Color> client in this.clients)
            // Process.Start(solutionDirectory + "/DADTKV/bin/Debug/net6.0/DADTKV.exe", client.First.ScriptPath);
        }

        private string getSolutionDirectory()
        {
            // Adapted from https://stackoverflow.com/a/11882118
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

            // Check if its the solution or if the project was open some other way
            do
            {
                string[] projects = new string[] { "ConfigParser", "Manager", "DADTKV", "LeaseManager", "TransactionManager" };
                if (projects.All(project => Directory.Exists(solutionDirectory + "/" + project)))
                    break;

                // If its not the correct directory, ask the user to select it or cancel
                DialogResult result = MessageBox.Show("The Manager.exe executable was not started from inside the Visual Studio solution.\nPlease select the solution directory, where the .sln file resides.", "Invalid Solution Directory", MessageBoxButtons.RetryCancel);
                if (result == DialogResult.Cancel)
                    Environment.Exit(0);

                // If the user selected retry, ask for the directory again
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select the solution directory";
                fbd.ShowDialog();
                solutionDirectory = fbd.SelectedPath;
            } while (true);

            return solutionDirectory;
        }
    }
}