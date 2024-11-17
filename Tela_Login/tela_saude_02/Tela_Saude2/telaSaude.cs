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
        private readonly string connectionString = "Server=mendonça\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;";
        private static HashSet<string> linhasAdicionadas = new HashSet<string>(); // Usado para armazenar identificadores de linhas já adicionadas
        private static HashSet<int> codigosGerados = new HashSet<int>(); // Usado para armazenar códigos gerados

        public telaSaude()
        {
            InitializeComponent();
            ConfigurarDataGridView();  // Configuração das colunas só uma vez
            Load += telaSaude_Load;
        }

        private void telaSaude_Load(object sender, EventArgs e)
        {
            // Obtém o nome da primeira espécie de plantação
            DataTable tiposPlantacao = ObterTiposDePlantacao();
            if (tiposPlantacao.Rows.Count > 0)
            {
                string tipoPlantacao = tiposPlantacao.Rows[0]["tipo_plantacao"].ToString();
                string especie = ObterEspeciePorTipoPlantacao(tipoPlantacao);  // Função para obter a espécie relacionada ao tipo de plantação

                // Atualiza o título do gráfico com a espécie e centraliza
                CentralizarTituloGrafico(especie);
                
            }
        }

        private void CentralizarTituloGrafico(string titulo)
        {
            // Supondo que 'grafico1' seja uma Label no painel onde o gráfico está
            titulografico1.Text = titulo;  // Define o texto da Label como a espécie

            titulografico1.Location = new Point(
                (painelgrafico1.Width - grafico1.Width) / 2, // Calcula a posição centralizando no painel
                grafico1.Location.Y  // Mantém a posição vertical original
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
                return result?.ToString() ?? "Espécie Desconhecida";  // Retorna a espécie ou "Desconhecida" se não encontrada
            }
        }


        private void ConfigurarDataGridView()
        {
            // Limpa as colunas antes de configurar as novas
            tabela_Doenca.Columns.Clear();
            tabela_pragas.Columns.Clear();

            // Criação da coluna ComboBox para tabela_Doenca
            var comboBoxColumnDoenca = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Plantação",
                DisplayMember = "tipo_plantacao",
                ValueMember = "tipo_plantacao"
            };

            // Obtém os tipos de plantação
            DataTable tiposPlantacao = ObterTiposDePlantacao();
            if (tiposPlantacao.Rows.Count == 0)
            {
                MessageBox.Show("Nenhum tipo de plantação encontrado no banco de dados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            comboBoxColumnDoenca.DataSource = tiposPlantacao;

            // Adiciona a coluna ComboBox na tabela_doenca
            tabela_Doenca.Columns.Add(comboBoxColumnDoenca);

            // Criação da coluna ComboBox para tabela_pragas
            var comboBoxColumnPragas = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Plantação",
                DisplayMember = "tipo_plantacao",
                ValueMember = "tipo_plantacao",
                DataSource = tiposPlantacao // Configura a mesma fonte de dados para tabela_pragas
            };

            // Adiciona a coluna ComboBox na tabela_pragas
            tabela_pragas.Columns.Add(comboBoxColumnPragas);

            // Adiciona outras colunas comuns
            AddCommonColumnsToGrid(tabela_Doenca);
            AddCommonColumnsToGrid(tabela_pragas);

            // Configuração visual para ambas as tabelas
            ConfigureGridViewStyle(tabela_Doenca);
            ConfigureGridViewStyle(tabela_pragas);
        }

        private void AddCommonColumnsToGrid(DataGridView grid)
        {
            grid.Columns.Add("nome_comum", "Nome Comum");
            grid.Columns.Add("nome_cientifico", "Nome Científico");
            grid.Columns.Add("data_deteccao", "Data de Detecção");
            grid.Columns.Add("eficacia", "Eficácia (%)");
            grid.Columns.Add("severidade", "Severidade");
            grid.Columns.Add("metodo_controle", "Método de Controle");
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
            grid.EditMode = DataGridViewEditMode.EditOnEnter;  // Permite edição ao clicar na célula
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
                codigoUnico = random.Next(10000000, 99999999);  // Gera um número aleatório entre 10 milhões e 99 milhões
            } while (codigosGerados.Contains(codigoUnico));  // Verifica se o código já foi gerado

            codigosGerados.Add(codigoUnico);  // Adiciona o código gerado ao conjunto para garantir que não se repita

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
                return result?.ToString();  // Retorna o código da plantação ou null se não encontrado
            }
        }

        private bool VerificarCamposObrigatorios(DataGridView grid)
        {
            foreach (DataGridViewRow row in grid.Rows) // Verifica para a tabela passada como parâmetro
            {
                if (row.IsNewRow) continue;

                // Obtém os valores das células
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

            return true; // Todos os campos obrigatórios estão preenchidos
        }

        private void btn_newLine_Doenca_Click(object sender, EventArgs e)
        {
            // Verifica se a linha atual foi preenchida corretamente antes de adicionar uma nova linha
            if (!VerificarCamposObrigatorios(tabela_Doenca))
            {
                MessageBox.Show("Por favor, preencha todos os campos antes de adicionar uma nova linha.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Impede a adição de uma nova linha
            }

            // Adiciona uma nova linha nas duas tabelas
            tabela_Doenca.Rows.Add();
        }

        private void btn_Adicionar_Click(object sender, EventArgs e)
        {
            // Verifica se todas as células de ambas as tabelas estão preenchidas
            if (!VerificarCamposObrigatorios(tabela_Doenca) || !VerificarCamposObrigatorios(tabela_pragas))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios antes de salvar.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (DataGridViewRow row in tabela_Doenca.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Cria um identificador único para a linha (usando os valores das colunas)
                    string identificadorLinha = $"{row.Cells["nome_comum"].Value}-{row.Cells["nome_cientifico"].Value}-{row.Cells["tipo_plantacao"].Value}";

                    if (linhasAdicionadas.Contains(identificadorLinha))
                    {
                        MessageBox.Show("Esta linha já foi adicionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue; // Pula a linha se já foi adicionada
                    }

                    // Caso contrário, adiciona a linha ao banco de dados
                    string tipoPlantacao = row.Cells["tipo_plantacao"].Value?.ToString();
                    string codPlantacao = ObterCodPlantacaoPorTipo(tipoPlantacao);
                    int codPragasDoencas = GerarCodigoUnico(); // Gera um código único para cada entrada

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

                    command.ExecuteNonQuery(); // Executa a inserção no banco de dados


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
                return; // Impede a adição de uma nova linha
            }

            // Adiciona uma nova linha nas duas tabelas
            tabela_pragas.Rows.Add();
        }



        private void AtualizarGraficoDeEficaciaESeveridade()
        {
            // Cria um dicionário para armazenar a soma e a quantidade de eficácia e severidade por espécie
            Dictionary<string, (double somaEficacia, double somaSeveridade, int quantidade)> especieDados = new Dictionary<string, (double somaEficacia, double somaSeveridade, int quantidade)>();

            // Consulta SQL para obter os valores de eficácia, severidade e as espécies da Plantacao
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
                        // Lê os valores de eficácia, severidade e a espécie
                        string especie = reader.IsDBNull(reader.GetOrdinal("especie")) ? "Desconhecida" : reader.GetString(reader.GetOrdinal("especie"));
                        decimal eficaciaDecimal = reader.IsDBNull(reader.GetOrdinal("eficacia")) ? 0 : reader.GetDecimal(reader.GetOrdinal("eficacia"));
                        string severidadeTexto = reader.IsDBNull(reader.GetOrdinal("severidade")) ? "0" : reader.GetString(reader.GetOrdinal("severidade"));

                        // Tenta converter severidade de varchar para decimal
                        decimal severidadeDecimal = 0;
                        if (!decimal.TryParse(severidadeTexto, out severidadeDecimal))
                        {
                            severidadeDecimal = 0; // Caso a conversão falhe, define 0 como padrão
                        }

                        // Verifica se a espécie já existe no dicionário, se não, inicializa
                        if (!especieDados.ContainsKey(especie))
                        {
                            especieDados[especie] = (0, 0, 0);
                        }

                        // Atualiza a soma de eficácia e severidade e a quantidade de registros por espécie
                        var dadosEspecie = especieDados[especie];
                        especieDados[especie] = (dadosEspecie.somaEficacia + (double)eficaciaDecimal,
                                                 dadosEspecie.somaSeveridade + (double)severidadeDecimal,
                                                 dadosEspecie.quantidade + 1);
                    }
                }
            }

            // Após acumular os dados, você pode fazer o cálculo das médias ou totais e atualizar o gráfico
            foreach (var especie in especieDados.Keys)
            {
                var dadosEspecie = especieDados[especie];
                double mediaEficacia = dadosEspecie.somaEficacia / dadosEspecie.quantidade;
                double mediaSeveridade = dadosEspecie.somaSeveridade / dadosEspecie.quantidade;

                // Atualize o gráfico ou interface com os dados agregados
                AtualizarCircleProgressBar(especie, dadosEspecie.somaEficacia, mediaEficacia, dadosEspecie.somaSeveridade, mediaSeveridade);
            }
        }

        private void AtualizarCircleProgressBar(string especie, double somaEficacia, double mediaEficacia, double somaSeveridade, double mediaSeveridade)
        {
            // Limita o valor para o intervalo adequado (0 a 100, por exemplo)
            int valorEficacia = (int)Math.Min(100, Math.Max(0, mediaEficacia));

            // Atualiza o valor do circleProgressBar
            grafico1.Value = valorEficacia;

            // Exibe a soma e média de eficácia e severidade no título do gráfico
            CentralizarTituloGrafico(titulografico1.Text = $"{especie}\nEficácia: {mediaEficacia:F2}, Severidade: {mediaSeveridade:F2}");
        }






    }
}