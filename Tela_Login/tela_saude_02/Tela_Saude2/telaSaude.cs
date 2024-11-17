using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Tela_Saude2
{
    public partial class telaSaude : Form
    {
        private readonly string connectionString = "Server=mendon�a\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;";
        private static HashSet<string> linhasAdicionadas = new HashSet<string>(); // Usado para armazenar identificadores de linhas j� adicionadas
        private static HashSet<int> codigosGerados = new HashSet<int>(); // Usado para armazenar c�digos gerados

        public telaSaude()
        {
            InitializeComponent();
            ConfigurarDataGridView();  // Configura��o das colunas s� uma vez
            Load += telaSaude_Load;
        }

        private void telaSaude_Load(object sender, EventArgs e)
        {
            // Obt�m o nome da primeira esp�cie de planta��o
            DataTable tiposPlantacao = ObterTiposDePlantacao();
            if (tiposPlantacao.Rows.Count > 0)
            {
                string tipoPlantacao = tiposPlantacao.Rows[0]["tipo_plantacao"].ToString();
                string especie = ObterEspeciePorTipoPlantacao(tipoPlantacao);  // Fun��o para obter a esp�cie relacionada ao tipo de planta��o

                // Atualiza o t�tulo do gr�fico com a esp�cie e centraliza
                CentralizarTituloGrafico(especie);
                
            }
        }

        private void CentralizarTituloGrafico(string titulo)
        {
            // Supondo que 'grafico1' seja uma Label no painel onde o gr�fico est�
            titulografico1.Text = titulo;  // Define o texto da Label como a esp�cie

            titulografico1.Location = new Point(
                (painelgrafico1.Width - grafico1.Width) / 2, // Calcula a posi��o centralizando no painel
                grafico1.Location.Y  // Mant�m a posi��o vertical original
            );
        }


        private string ObterEspeciePorTipoPlantacao(string tipoPlantacao)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT especie FROM Plantacao WHERE tipo_plantacao = @tipo_plantacao";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tipo_plantacao", tipoPlantacao);

                connection.Open();
                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Esp�cie Desconhecida";  // Retorna a esp�cie ou "Desconhecida" se n�o encontrada
            }
        }


        private void ConfigurarDataGridView()
        {
            // Limpa as colunas antes de configurar as novas
            tabela_Doenca.Columns.Clear();
            tabela_pragas.Columns.Clear();

            // Cria��o da coluna ComboBox para tabela_Doenca
            var comboBoxColumnDoenca = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Planta��o",
                DisplayMember = "tipo_plantacao",
                ValueMember = "tipo_plantacao"
            };

            // Obt�m os tipos de planta��o
            DataTable tiposPlantacao = ObterTiposDePlantacao();
            if (tiposPlantacao.Rows.Count == 0)
            {
                MessageBox.Show("Nenhum tipo de planta��o encontrado no banco de dados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            comboBoxColumnDoenca.DataSource = tiposPlantacao;

            // Adiciona a coluna ComboBox na tabela_doenca
            tabela_Doenca.Columns.Add(comboBoxColumnDoenca);

            // Cria��o da coluna ComboBox para tabela_pragas
            var comboBoxColumnPragas = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Planta��o",
                DisplayMember = "tipo_plantacao",
                ValueMember = "tipo_plantacao",
                DataSource = tiposPlantacao // Configura a mesma fonte de dados para tabela_pragas
            };

            // Adiciona a coluna ComboBox na tabela_pragas
            tabela_pragas.Columns.Add(comboBoxColumnPragas);

            // Adiciona outras colunas comuns
            AddCommonColumnsToGrid(tabela_Doenca);
            AddCommonColumnsToGrid(tabela_pragas);

            // Configura��o visual para ambas as tabelas
            ConfigureGridViewStyle(tabela_Doenca);
            ConfigureGridViewStyle(tabela_pragas);
        }

        private void AddCommonColumnsToGrid(DataGridView grid)
        {
            grid.Columns.Add("nome_comum", "Nome Comum");
            grid.Columns.Add("nome_cientifico", "Nome Cient�fico");
            grid.Columns.Add("data_deteccao", "Data de Detec��o");
            grid.Columns.Add("eficacia", "Efic�cia (%)");
            grid.Columns.Add("severidade", "Severidade");
            grid.Columns.Add("metodo_controle", "M�todo de Controle");
        }

        private void ConfigureGridViewStyle(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116);
                column.HeaderCell.Style.ForeColor = Color.White;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            grid.RowTemplate.Height = 20;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.Width = 150;
            }

            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EnableHeadersVisualStyles = false;
            grid.EditMode = DataGridViewEditMode.EditOnEnter;  // Permite edi��o ao clicar na c�lula
        }

        private DataTable ObterTiposDePlantacao()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT tipo_plantacao FROM Plantacao";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private int GerarCodigoUnico()
        {
            Random random = new Random();
            int codigoUnico;

            do
            {
                codigoUnico = random.Next(10000000, 99999999);  // Gera um n�mero aleat�rio entre 10 milh�es e 99 milh�es
            } while (codigosGerados.Contains(codigoUnico));  // Verifica se o c�digo j� foi gerado

            codigosGerados.Add(codigoUnico);  // Adiciona o c�digo gerado ao conjunto para garantir que n�o se repita

            return codigoUnico;
        }

        private string ObterCodPlantacaoPorTipo(string tipoPlantacao)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT cod_plantacao FROM Plantacao WHERE tipo_plantacao = @tipo_plantacao";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tipo_plantacao", tipoPlantacao);

                connection.Open();
                var result = command.ExecuteScalar();
                return result?.ToString();  // Retorna o c�digo da planta��o ou null se n�o encontrado
            }
        }

        private bool VerificarCamposObrigatorios(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.Rows) // Verifica para a tabela passada como par�metro
            {
                if (row.IsNewRow) continue;

                // Obt�m os valores das c�lulas
                string nomeComum = row.Cells["nome_comum"].Value?.ToString();
                string nomeCientifico = row.Cells["nome_cientifico"].Value?.ToString();
                string tipoPlantacao = row.Cells["tipo_plantacao"].Value?.ToString();
                string dataDeteccao = row.Cells["data_deteccao"].Value?.ToString();
                string eficacia = row.Cells["eficacia"].Value?.ToString();
                string severidade = row.Cells["severidade"].Value?.ToString();
                string metodoControle = row.Cells["metodo_controle"].Value?.ToString();

                if (string.IsNullOrEmpty(nomeComum) || string.IsNullOrEmpty(nomeCientifico) ||
                    string.IsNullOrEmpty(tipoPlantacao) || string.IsNullOrEmpty(dataDeteccao) ||
                    string.IsNullOrEmpty(eficacia) || string.IsNullOrEmpty(severidade) ||
                    string.IsNullOrEmpty(metodoControle))
                {
                    return false; // Se algum campo estiver vazio, retorna falso
                }
            }

            return true; // Todos os campos obrigat�rios est�o preenchidos
        }

        private void btn_newLine_Doenca_Click(object sender, EventArgs e)
        {
            // Verifica se a linha atual foi preenchida corretamente antes de adicionar uma nova linha
            if (!VerificarCamposObrigatorios(tabela_Doenca))
            {
                MessageBox.Show("Por favor, preencha todos os campos antes de adicionar uma nova linha.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Impede a adi��o de uma nova linha
            }

            // Adiciona uma nova linha nas duas tabelas
            tabela_Doenca.Rows.Add();
        }

        private void btn_Adicionar_Click(object sender, EventArgs e)
        {
            // Verifica se todas as c�lulas de ambas as tabelas est�o preenchidas
            if (!VerificarCamposObrigatorios(tabela_Doenca) || !VerificarCamposObrigatorios(tabela_pragas))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigat�rios antes de salvar.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (DataGridViewRow row in tabela_Doenca.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Cria um identificador �nico para a linha (usando os valores das colunas)
                    string identificadorLinha = $"{row.Cells["nome_comum"].Value}-{row.Cells["nome_cientifico"].Value}-{row.Cells["tipo_plantacao"].Value}";

                    if (linhasAdicionadas.Contains(identificadorLinha))
                    {
                        MessageBox.Show("Esta linha j� foi adicionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue; // Pula a linha se j� foi adicionada
                    }

                    // Caso contr�rio, adiciona a linha ao banco de dados
                    string tipoPlantacao = row.Cells["tipo_plantacao"].Value?.ToString();
                    string codPlantacao = ObterCodPlantacaoPorTipo(tipoPlantacao);
                    int codPragasDoencas = GerarCodigoUnico(); // Gera um c�digo �nico para cada entrada

                    string query = "INSERT INTO Controle_Pragas_Doencas (cod_pragas_doencas, cod_plantacao, nome_comum, nome_cientifico, data_deteccao, eficacia, severidade, metodo_controle) " +
                                   "VALUES (@cod_pragas_doencas, @cod_plantacao, @nome_comum, @nome_cientifico, @data_deteccao, @eficacia, @severidade, @metodo_controle)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@cod_pragas_doencas", codPragasDoencas);
                    command.Parameters.AddWithValue("@cod_plantacao", codPlantacao);
                    command.Parameters.AddWithValue("@nome_comum", row.Cells["nome_comum"].Value);
                    command.Parameters.AddWithValue("@nome_cientifico", row.Cells["nome_cientifico"].Value);

                    command.Parameters.AddWithValue("@data_deteccao", row.Cells["data_deteccao"].Value);
                    command.Parameters.AddWithValue("@eficacia", row.Cells["eficacia"].Value);
                    command.Parameters.AddWithValue("@severidade", row.Cells["severidade"].Value);
                    command.Parameters.AddWithValue("@metodo_controle", row.Cells["metodo_controle"].Value);

                    command.ExecuteNonQuery(); // Executa a inser��o no banco de dados


                }
            }
            AtualizarGraficoDeEficaciaESeveridade();
            MessageBox.Show("Dados inseridos com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btn_pragas_Click(object sender, EventArgs e)
        {
            // Verifica se a linha atual foi preenchida corretamente antes de adicionar uma nova linha
            if (!VerificarCamposObrigatorios(tabela_pragas))
            {
                MessageBox.Show("Por favor, preencha todos os campos antes de adicionar uma nova linha.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Impede a adi��o de uma nova linha
            }

            // Adiciona uma nova linha nas duas tabelas
            tabela_pragas.Rows.Add();
        }



        private void AtualizarGraficoDeEficaciaESeveridade()
        {
            // Cria um dicion�rio para armazenar a soma e a quantidade de efic�cia e severidade por esp�cie
            Dictionary<string, (double somaEficacia, double somaSeveridade, int quantidade)> especieDados = new Dictionary<string, (double somaEficacia, double somaSeveridade, int quantidade)>();

            // Consulta SQL para obter os valores de efic�cia, severidade e as esp�cies da Plantacao
            string query = @"
    SELECT p.especie, c.eficacia, c.severidade
    FROM Controle_Pragas_Doencas c
    INNER JOIN Plantacao p ON c.cod_plantacao = p.cod_plantacao";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // L� os valores de efic�cia, severidade e a esp�cie
                        string especie = reader.IsDBNull(reader.GetOrdinal("especie")) ? "Desconhecida" : reader.GetString(reader.GetOrdinal("especie"));
                        decimal eficaciaDecimal = reader.IsDBNull(reader.GetOrdinal("eficacia")) ? 0 : reader.GetDecimal(reader.GetOrdinal("eficacia"));
                        string severidadeTexto = reader.IsDBNull(reader.GetOrdinal("severidade")) ? "0" : reader.GetString(reader.GetOrdinal("severidade"));

                        // Tenta converter severidade de varchar para decimal
                        decimal severidadeDecimal = 0;
                        if (!decimal.TryParse(severidadeTexto, out severidadeDecimal))
                        {
                            severidadeDecimal = 0; // Caso a convers�o falhe, define 0 como padr�o
                        }

                        // Verifica se a esp�cie j� existe no dicion�rio, se n�o, inicializa
                        if (!especieDados.ContainsKey(especie))
                        {
                            especieDados[especie] = (0, 0, 0);
                        }

                        // Atualiza a soma de efic�cia e severidade e a quantidade de registros por esp�cie
                        var dadosEspecie = especieDados[especie];
                        especieDados[especie] = (dadosEspecie.somaEficacia + (double)eficaciaDecimal,
                                                 dadosEspecie.somaSeveridade + (double)severidadeDecimal,
                                                 dadosEspecie.quantidade + 1);
                    }
                }
            }

            // Ap�s acumular os dados, voc� pode fazer o c�lculo das m�dias ou totais e atualizar o gr�fico
            foreach (var especie in especieDados.Keys)
            {
                var dadosEspecie = especieDados[especie];
                double mediaEficacia = dadosEspecie.somaEficacia / dadosEspecie.quantidade;
                double mediaSeveridade = dadosEspecie.somaSeveridade / dadosEspecie.quantidade;

                // Atualize o gr�fico ou interface com os dados agregados
                AtualizarCircleProgressBar(especie, dadosEspecie.somaEficacia, mediaEficacia, dadosEspecie.somaSeveridade, mediaSeveridade);
            }
        }

        private void AtualizarCircleProgressBar(string especie, double somaEficacia, double mediaEficacia, double somaSeveridade, double mediaSeveridade)
        {
            // Limita o valor para o intervalo adequado (0 a 100, por exemplo)
            int valorEficacia = (int)Math.Min(100, Math.Max(0, mediaEficacia));

            // Atualiza o valor do circleProgressBar
            grafico1.Value = valorEficacia;

            // Exibe a soma e m�dia de efic�cia e severidade no t�tulo do gr�fico
            CentralizarTituloGrafico(titulografico1.Text = $"{especie}\nEfic�cia: {mediaEficacia:F2}, Severidade: {mediaSeveridade:F2}");
        }






    }
}