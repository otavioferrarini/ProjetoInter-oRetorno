using System;
using System.Collections.Generic;
using Npgsql;
using NodaTime;
using Npgsql.NodaTime;

namespace RastreamentoCoronavirus
{
    class BancoHandler
    {   
        /*
        A conexao com o banco, a alma da classe inteira. TUDO precisa disso. 
        Altere a senha do usuario padrão 'postgres' no PostgreSQL usando este comando:
        ALTER ROLE postgres WITH PASSWORD 'stdpass';
        Assim voce conseguirá acessar o banco.
        */
        public readonly NpgsqlConnection conexao = new NpgsqlConnection("Host=127.0.0.1;Username=postgres;Password=stdpass;Database=rastreamentocoronavirus");
        
        //a tabela usuario, pra nao ocorrer ambiguidade ou erro de escrita nos metodos.
        public readonly string tabelaUsuario = "usuario";

        //a tabela estabelecimento, pra nao ocorrer ambiguidade ou erro de escrita nos metodos.
        public readonly string tabelaEstabelecimento = "estabelecimento";

        //a tabela checkin, pra nao ocorrer ambiguidade ou erro de escrita nos metodos.
        public readonly string tabelaCheckin = "checkin";

        //a tabela reportado, pra nao ocorrer ambiguidade ou erro de escrita nos metodos.
        public readonly string tabelaReportado = "reportado";
        
        //o modo update, pra nao ocorrer ambiguidade ou erro de escrita no metodo edicao.
        public readonly string modoUpdate = "UPDATE";
        
         //o modo delete, pra nao ocorrer ambiguidade ou erro de escrita no metodo edicao.
        public readonly string modoDelete = "DELETE";
        
        //a tabela tel_usuario, pra nao ocorrer ambiguidade ou erro de escrita no metodo telefone.
        public readonly string telUsuario = "tel_usuario";
        
        //a tabela tel_estabel, pra nao ocorrer ambiguidade ou erro de escrita no metodo telefone.
        public readonly string telEstabelecimento = "tel_estabel";

        
        
        /*metodo para realizacao do cadastro nas tabelas, tanto na usuario quanto na estabelecimento,
          ja que elas tem a mesma estrutura. usa uma lista para receber os dados.*/
        public void Cadastro(string tabela, List<string> vals, NpgsqlConnection conn){
            string copyfrom = "COPY " + tabela + " FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copyfrom)){
                writer.StartRow();
                foreach(string v in vals){
                    if(v == "") writer.WriteNull();
                    else writer.Write(v);
                }
                writer.Complete();
                Console.WriteLine("Cadastrado!");
            }
        }

        //metodo para cadastro dos telefones, tanto do usuario, quanto do estabelecimento.
        public void Telefone(string tabela, string cod, List<string> tels, NpgsqlConnection conn){
            string copyfrom = "COPY " + tabela + " FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copyfrom)){            
                for(int i = 0;i < tels.Count;i++){
                    writer.StartRow();
                    writer.Write(cod);
                    writer.Write(i+1);
                    writer.Write(tels[i]);
                }
                writer.Complete();
                Console.WriteLine("Cadastrado!");
            }
        }

        //metodo util para receber todos os CPFs ja registrados. retorna uma lista.
        public List<string> GetCPFs(NpgsqlConnection conn){
            using (NpgsqlCommand comando = new NpgsqlCommand("SELECT cpf FROM usuario", conn))
            using (NpgsqlDataReader reader = comando.ExecuteReader()){
                List<string> cpfs = new List<string>();
                while(reader.Read()){
                    cpfs.Add(reader.GetString(0));
                }
                return cpfs;
            }
        }

        //metodo util para receber todos os CNPJs ja registrados. retorna uma lista.
        public List<string> GetCNPJs(NpgsqlConnection conn){
            using (NpgsqlCommand comando = new NpgsqlCommand("SELECT cnpj FROM estabelecimento", conn))
            using (NpgsqlDataReader reader = comando.ExecuteReader()){
                List<string> cpfs = new List<string>();
                while(reader.Read()){
                    cpfs.Add(reader.GetString(0));
                }
                return cpfs;
            }
        }

        //metodo pra registrar na tabela checkin, agora com a data.
        public void Checkin(string cpf, List<string> vci, NpgsqlConnection conn){
            conn.TypeMapper.UseNodaTime();
            string[] data = vci[1].Split('-', 3);
            LocalDate dataComp = new LocalDate(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
            string copyfrom = "COPY checkin (\"cpf\", \"cnpj\", \"data\") FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copyfrom)){
                writer.StartRow();
                writer.Write(cpf);
                writer.Write(vci[0]);
                writer.Write(dataComp);
                writer.Complete();
                Console.WriteLine("Cadastrado!");
            }
        }

        /*metodo para executar a consulta de checar risco de contaminacao em um estabelecimento.
          retorna um dicionario com um cpf em risco como chave e a razao social do local como valor.
          agora com comparacao de datas*/
        public Dictionary<string,string> ChecaRisco(NpgsqlConnection conn){
            Dictionary<string, string> emrisco = new Dictionary<string, string>();
            string strcmd = @"WITH rep AS (SELECT c.cpf, c.cnpj, r.data as datarep, c.data as datacheck FROM reportado r INNER JOIN checkin c ON c.cpf = r.cpf)
                SELECT r.cpf, e.RazaoSocial
                FROM rep r LEFT JOIN estabelecimento e 
                ON r.cnpj = e.cnpj 
                WHERE r.datacheck + interval '7 days' > r.datarep;";
            using (NpgsqlCommand comando = new NpgsqlCommand(strcmd, conn))
            using (NpgsqlDataReader reader = comando.ExecuteReader()){
                while(reader.Read()){
                    if(!(emrisco.ContainsKey(reader.GetString(0)))) 
                        emrisco.Add(reader.GetString(0), reader.GetString(1));
                }
            }
            return emrisco;

        }

        //metodo para cadastro no reportar. recebe o cpf e a data no formato 'yyyy-mm-dd'
        public void Reportar(string cpf, string dt, NpgsqlConnection conn){
            conn.TypeMapper.UseNodaTime();
            string[] data = dt.Split('-', 3);
            LocalDate dataComp = new LocalDate(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
            string copyfrom = "COPY reportado FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copyfrom)){
                writer.StartRow();
                writer.Write(cpf);
                writer.Write(dataComp);
                writer.Complete();
                Console.WriteLine("Cadastrado!");
            }
        }

        /*metodo que permite executar um update de uma coluna de um registro, 
          usando a coluna afetada e um novo valor para ela, e uma condicao, caso necessario. 
          permite tambem executar um delete em uma tabela, utiliza apenas a tabela e a condicao.
          (ta bem bagunçado e deve ter um jeito melhor de fazer isso... mas funciona!)*/
        public void Edicao(string tabela, string modo, string col, string val, string cond, NpgsqlConnection conn){
            using (NpgsqlCommand comando = new NpgsqlCommand()){
                comando.Connection = conn;
                string strcmd = "";
                if(modo == "UPDATE"){
                    strcmd = "UPDATE " + tabela + " SET " + col + " = " + "'" + val + "'" + " WHERE " + cond + ";";
                    
                    comando.CommandText = @strcmd;

                }
                else if(modo == "DELETE"){
                    strcmd = "DELETE FROM " + tabela + " WHERE " + cond + ";";

                    comando.CommandText = @strcmd;
                }

                int rows = comando.ExecuteNonQuery();
                Console.WriteLine("Sucesso! Linhas afetadas: " + rows);
            }
        }
        
        /*metodo para listar uma tabela inteira, retorna um dicionario com cada chave representando uma coluna,
          e cada valor sendo o registro desta coluna.
          caso o registro neja nulo, o registro vai pra lista como "NULL"*/
        public Dictionary<int, List<string>> Listagem(string tabela, NpgsqlConnection conn){
            Dictionary<int, List<string>> dout = new Dictionary<int, List<string>>();
            string strcmd = "SELECT * FROM " + tabela + ";";

            dout.Add(0, new List<string>());
            dout.Add(1, new List<string>());
            dout.Add(2, new List<string>());
            dout.Add(3, new List<string>());
            dout.Add(4, new List<string>());
            dout.Add(5, new List<string>());
            dout.Add(6, new List<string>());
            dout.Add(7, new List<string>());
            dout.Add(8, new List<string>());

            using (NpgsqlCommand comando = new NpgsqlCommand(strcmd, conn))
            using (NpgsqlDataReader reader = comando.ExecuteReader()){
                while(reader.Read()){
                    dout[0].Add(reader.GetString(0));//not null
                    dout[1].Add(reader.GetString(1));//not null
                    if(reader.IsDBNull(2)) dout[2].Add("NULL");
                    else dout[2].Add(reader.GetString(2));
                    if(reader.IsDBNull(3)) dout[3].Add("NULL");
                    else dout[3].Add(reader.GetString(3));
                    if(reader.IsDBNull(4)) dout[4].Add("NULL");
                    else dout[4].Add(reader.GetString(4));
                    dout[5].Add(reader.GetString(5));//not null
                    if(reader.IsDBNull(6)) dout[6].Add("NULL");
                    else dout[6].Add(reader.GetString(6));
                    if(reader.IsDBNull(7)) dout[7].Add("NULL");
                    else dout[7].Add(reader.GetString(7));
                    if(reader.IsDBNull(8)) dout[8].Add("NULL");
                    else dout[8].Add(reader.GetString(8));
                }
                return dout;
            }
        }

        //metodo para juntar todos os checkins em um dicionario. chave é o cpf, valor é o codcheckin
        public Dictionary<string,int> GetCodCheckins(NpgsqlConnection conn){
            Dictionary<string,int> dout = new Dictionary<string,int>();
            string strcmd = "SELECT u.cpf, c.codcheckin FROM usuario as u JOIN checkin as c on u.cpf = c.cpf;";
            using (NpgsqlCommand cmd = new NpgsqlCommand(strcmd, conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader()){
                while(reader.Read()){
                    if (reader.IsDBNull(0)) dout.Add("NULL", 0);
                    else dout.Add(reader.GetString(0), reader.GetInt32(1));   
                }
            }
            return dout;
        }
        
    }
}
