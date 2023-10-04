using Manager.Manager;
using Manager.StatusHook;
using Parser;
using Parser.Parsers;
using System.Diagnostics;
using System.Threading;

namespace Manager
{
    public partial class Main : Form
    {
        public static List<Pair<ClientConfigLine, Color>> Clients { get; private set; } = new List<Pair<ClientConfigLine, Color>>();
        public static List<Pair<ServerConfigLine, Color>> TransactionManagers { get; private set; } = new List<Pair<ServerConfigLine, Color>>();
        public static List<Pair<ServerConfigLine, Color>> LeaseManagers { get; private set; } = new List<Pair<ServerConfigLine, Color>>();

        private ConfigParser? config = null;
        private System.Threading.Timer? processTimer = null;

        public Main()
        {
            InitializeComponent();
        }

        private void updateStatuses(object state)
        {
            // Performance could be improved by only updating the items that changed when they changed
            // But this works fine for the time being

            // Clients
            this.lsvwClients.BeginUpdate();
            if (this.lsvwClients.Items.Count != Main.Clients.Count)
            {
                this.lsvwClients.Items.Clear();
                this.lsvwClients.Items.AddRange(Main.Clients.Select(client => new ListViewItem()
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
                    this.lsvwClients.Items[i].BackColor = Main.Clients[i].Second;
                }
            }
            this.lsvwClients.EndUpdate();

            // TMs
            this.lsvwTMs.BeginUpdate();
            if (this.lsvwTMs.Items.Count != Main.TransactionManagers.Count)
            {
                this.lsvwTMs.Items.Clear();
                this.lsvwTMs.Items.AddRange(TransactionManagers.Select(server => new ListViewItem()
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
                    this.lsvwTMs.Items[i].BackColor = Main.TransactionManagers[i].Second;
                }
            }
            this.lsvwTMs.EndUpdate();

            // LMs
            this.lsvwLMs.BeginUpdate();
            if (this.lsvwLMs.Items.Count != Main.LeaseManagers.Count)
            {
                this.lsvwLMs.Items.Clear();
                this.lsvwLMs.Items.AddRange(Main.LeaseManagers.Select(server => new ListViewItem()
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
                    this.lsvwLMs.Items[i].BackColor = Main.LeaseManagers[i].Second;
                }
            }
            this.lsvwLMs.EndUpdate();
        }

        private void Entry_Load(object sender, EventArgs e)
        {
            // Set labels
            this.lblClientsLabel.Text = "Gray = Not Started\nYellow = Idle\nGreen = Sending requests";
            this.lblTMsLabel.Text = "Gray = Not Started\nYellow = Idle (waiting for a lease)\nRed = Crashed\nLight Green = Executing transaction\nDark Green = Commiting Transaction";
            this.lblLMsLabel.Text = "Gray = Not Started\nYellow = Idle\nRed = Crashed\nLight Green = Paxos Proposer\nDark Green = Paxos Acceptor";

            this.fdConfig.Title = "Select the configuration file";
            this.fdConfig.FileName = "config.txt"; // Default name
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
                Environment.Exit(1);
            }
        }

        private void loadConfig(String configFilePath)
        {
            this.config = new ConfigParser(configFilePath);
            this.config.Parse();

            lock (Main.Clients)
            {
                Main.Clients = this.config.Config[ConfigType.Client]
                    .Select(x => new Pair<ClientConfigLine, Color>((ClientConfigLine)x, ClientStatus.NotStarted))
                    .ToList();
            }

            lock (Main.TransactionManagers)
            {
                Main.TransactionManagers = this.config.Config[ConfigType.Server]
                    .Select(x => new Pair<ServerConfigLine, Color>((ServerConfigLine)x, TMStatus.NotStarted))
                                   .Where(server => server.First.Type == ServerType.TransactionManager)
                                   .ToList();
            }

            lock (Main.LeaseManagers)
            {
                Main.LeaseManagers = this.config.Config[ConfigType.Server]
                    .Select(x => new Pair<ServerConfigLine, Color>((ServerConfigLine)x, LMStatus.NotStarted))
                                .Where(server => server.First.Type == ServerType.LeaseManager)
                                .ToList();
            }

            this.startProcessMonitoring();
            this.startProcesses();
        }

        private void startProcessMonitoring()
        {
            // Update GUI every second
            this.processTimer = new System.Threading.Timer(this.updateStatuses!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void stopProcessMonitoring()
        {
            // TODO notify peers to stop sending status updates
            this.processTimer?.Dispose();
            this.processTimer = null;
        }

        private void startProcesses()
        {
            string solutionDirectory = this.getSolutionDirectory();

            // Start LMs
            foreach (Pair<ServerConfigLine, Color> lm in Main.LeaseManagers)
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

        private void lsvwLMs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem focusedItem = lsvwLMs.FocusedItem;
                if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
                {
                    this.ctxLMs.Show(Cursor.Position);
                }
            }
        }

        private void crashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem focusedItem = lsvwLMs.FocusedItem;
            if (focusedItem == null) return;

            string id = focusedItem.Text;
            Pair<ServerConfigLine, Color> lm = Main.LeaseManagers.Find(lm => lm.First.ID == id)!;
            if (lm == null) return;

            // TODO don't create everytime
            new ManagerClient().Crash(lm.First.Url);
        }
    }

    class ClientStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color SendingRequests = Color.Green;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "SendingRequests" };
    }

    class TMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color ExecutingTransaction = Color.LightGreen;
        public static readonly Color CommitingTransaction = Color.DarkGreen;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "Crashed", "ExecutingTransaction", "CommitingTransaction" };
    }

    class LMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color PaxosProposer = Color.LightGreen;
        public static readonly Color PaxosAcceptor = Color.DarkGreen;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "Crashed", "PaxosProposer", "PaxosAcceptor" };
    }

}