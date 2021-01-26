using System;
using System.Collections.Generic;
using Npgsql;
using NodaTime;

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
        
        //o modo update, pra nao ocorrer ambiguidade ou erro de escrita no metodo edicao.
        public readonly string modoUpdate = "UPDATE";
        
         //o modo delete, pra nao ocorrer ambiguidade ou erro de escrita no metodo edicao.
        public readonly string modoDelete = "DELETE";
        
        //a tabela tel_usuario, pra nao ocorrer ambiguidade ou erro de escrita nos metodo telefone.
        public readonly string telUsuario = "tel_usuario";
        
        //a tabela tel_estabel, pra nao ocorrer ambiguidade ou erro de escrita nos metodo telefone.
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

        //metodo pra registrar na tabela checkin, falta implementar a data, no momento a data sempre é enviada nula.
        public void Checkin(string cpf, List<string> vci, NpgsqlConnection conn){
            string copyfrom = "COPY checkin (\"cpf\", \"cnpj\", \"data\", \"teste\") FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copyfrom)){
                writer.StartRow();
                writer.Write(cpf);
                writer.Write(vci[0]);
                writer.WriteNull();
                writer.Write(vci[1]);
                writer.Complete();
                Console.WriteLine("Cadastrado!");
            }
        }

        /*metodo para executar a consulta de checar risco de contaminacao em um estabelecimento.
          retorna um dicionario com um cpf em risco como chave e a razao social do local como valor.
          falta implementar a funcionalidade da data.*/
        public Dictionary<string,string> ChecaRisco(NpgsqlConnection conn){
            List<string> cnpj = GetCNPJs(conn);
            Dictionary<string, string> emrisco = new Dictionary<string, string>();
            foreach(string v in cnpj){
                string strcmd = "WITH localcont AS (SELECT cnpj FROM checkin WHERE (teste = 'Positivo') AND (cnpj = '" + v + "')) SELECT c.cpf, e.RazaoSocial FROM checkin AS c LEFT JOIN estabelecimento AS e ON c.cnpj = e.cnpj WHERE (c.cnpj = (SELECT cnpj FROM localcont) AND (c.teste = 'Negativo'));";
                using (NpgsqlCommand comando = new NpgsqlCommand(strcmd, conn))
                using (NpgsqlDataReader reader = comando.ExecuteReader()){
                    while(reader.Read()){
                        if(!(emrisco.ContainsKey(reader.GetString(0)))) 
                            emrisco.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
            return emrisco;

        }

        //public void reportar() //ainda não implementado

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
                    
                    comando.CommandText = strcmd;

                }
                else if(modo == "DELETE"){
                    strcmd = "DELETE FROM " + tabela + " WHERE " + cond + ";";

                    comando.CommandText = strcmd;
                }

                int rows = comando.ExecuteNonQuery();
                Console.WriteLine("Sucesso! Linhas afetadas: " + rows);
            }
        }
        
        /*metodo para listar uma tabela inteira, retorna uma lista com cada valor sendo o registro de uma coluna da tabela.
          caso o registro neja nulo, o registro vai pra lista como "NULL"*/
        public List<string> Listagem(string tabela, NpgsqlConnection conn){
            string strcmd = "SELECT * FROM " + tabela + ";";           
            using (NpgsqlCommand comando = new NpgsqlCommand(strcmd, conn))
            using (NpgsqlDataReader reader = comando.ExecuteReader()){
                List<string> lout = new List<string>();
                while(reader.Read()){
                    lout.Add(reader.GetString(0));//not null
                    lout.Add(reader.GetString(1));//not null
                    if(reader.IsDBNull(2)) lout.Add("NULL");
                    else lout.Add(reader.GetString(2));
                    if(reader.IsDBNull(3)) lout.Add("NULL");
                    else lout.Add(reader.GetString(3));
                    if(reader.IsDBNull(4)) lout.Add("NULL");
                    else lout.Add(reader.GetString(4));
                    lout.Add(reader.GetString(5));//not null
                    if(reader.IsDBNull(6)) lout.Add("NULL");
                    else lout.Add(reader.GetString(6));
                    if(reader.IsDBNull(7)) lout.Add("NULL");
                    else lout.Add(reader.GetString(7));
                    if(reader.IsDBNull(8)) lout.Add("NULL");
                    else lout.Add(reader.GetString(8));
                }
                return lout;
            }
        }
    }
}
