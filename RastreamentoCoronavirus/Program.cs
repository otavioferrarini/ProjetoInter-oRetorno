using System;
using System.Collections.Generic;
using Npgsql;

namespace RastreamentoCoronavirus
{
    class Program
    {
        static void Main(string[] args)
        {
            BancoHandler banco = new BancoHandler();
            NpgsqlConnection conexao = banco.conexao;
            List<string> vals = new List<string>();
            
            try{
                using (conexao){
                    conexao.Open();

                    Console.WriteLine("TESTES DOS MÉTODOS: \n");

                    vals.Add("12948302817");
                    vals.Add("ola amigo");
                    vals.Add("Somebody once told me the world");
                    vals.Add("15");
                    vals.Add("gonna roll me");
                    vals.Add("93026406");
                    vals.Add("");
                    vals.Add("Campinas");
                    vals.Add("SP");

                    banco.Cadastro(banco.tabelaUsuario, vals, conexao);

                    vals.Clear();

                    vals.Add("95730274937493");
                    vals.Add("i aint the sharpest tool in the shed");
                    vals.Add("she was looking kinda dumb with");
                    vals.Add("434");
                    vals.Add("finger and her thumb");
                    vals.Add("13009283");
                    vals.Add("");
                    vals.Add("Campinas");
                    vals.Add("SP");

                    banco.Cadastro(banco.tabelaEstabelecimento, vals, conexao);

                    vals.Clear();

                    vals.Add("11121314151");
                    vals.Add("17936281908");

                    banco.Telefone(banco.telUsuario, banco.GetCPFs(conexao)[0], vals, conexao);

                    vals.Clear();

                    var cpfs = banco.GetCPFs(conexao);
                    var cnpjs = banco.GetCNPJs(conexao);
                    
                    vals.Add(cnpjs[0]);
                    vals.Add("Negativo");

                    banco.Checkin(cpfs[0], vals, conexao);

                    vals.Clear();

                    vals.Add(cnpjs[0]);
                    vals.Add("Positivo");

                    banco.Checkin(cpfs[0], vals, conexao);

                    vals.Clear();

                    foreach(KeyValuePair<string,string> v in banco.ChecaRisco(conexao)){
                        Console.WriteLine("CPF do usuario em risco: {0} Nome do local contaminado: {1}", v.Key, v.Value);
                    }

                    banco.Edicao(banco.tabelaUsuario, banco.modoUpdate, "nome", "novonome", "cpf = '11111111111'", conexao);
                    //banco.Edicao(banco.tabelaEstabelecimento, banco.modoDelete, "", "", "cnpj = \'" + cnpjs[0] + "\'", conexao);

                    var listg = banco.Listagem(banco.tabelaUsuario, conexao);
                    Console.WriteLine(listg.Count);

                    /*gambiarra pra tirar os valores da lista. a linha do cpf sempre vai ter um cpf, 
                      seguindo a formula no indice da lista, assim como as outras linhas.
                      deve ter um jeito com a propria classe da lista de fazer isso de forma melhor... vou ver se acho algo*/
                    for(int i = 0;i < listg.Count/9;i++){
                        Console.WriteLine("CPF: " + listg[(9*i)]);
                        Console.WriteLine("nome: " + listg[(9*i) + 1]);
                        Console.WriteLine("rua: " + listg[(9*i) + 2]);
                        Console.WriteLine("n: " + listg[(9*i) + 3]);
                        Console.WriteLine("bairro: " + listg[(9*i) + 4]);
                        Console.WriteLine("Cep: " + listg[(9*i) + 5]);
                        Console.WriteLine("Complemento: " + listg[(9*i) + 6]);
                        Console.WriteLine("Cidade: " + listg[(9*i) + 7]);
                        Console.WriteLine("estado: " + listg[(9*i) + 8]);
                    }

                    var liste = banco.Listagem(banco.tabelaEstabelecimento, conexao);
                    Console.WriteLine(listg.Count);

                    for(int i = 0;i < listg.Count/9;i++){
                        Console.WriteLine("CNPJ: " + listg[(9*i)]);
                        Console.WriteLine("razao social: " + listg[(9*i) + 1]);
                        Console.WriteLine("rua: " + listg[(9*i) + 2]);
                        Console.WriteLine("n: " + listg[(9*i) + 3]);
                        Console.WriteLine("bairro: " + listg[(9*i) + 4]);
                        Console.WriteLine("Cep: " + listg[(9*i) + 5]);
                        Console.WriteLine("Complemento: " + listg[(9*i) + 6]);
                        Console.WriteLine("Cidade: " + listg[(9*i) + 7]);
                        Console.WriteLine("estado: " + listg[(9*i) + 8]);
                    }
                }
            }
            catch (PostgresException erro)
            {
                switch (erro.SqlState)
                {
                    case "28P01":
                        Console.WriteLine("Erro de Autenticação!");
                        Console.WriteLine("Verifique se o nome do usuário ou senha estão corretos!");
                        break;

                    case "3D000":
                        Console.WriteLine("Banco não encontrado no Servidor");
                        Console.WriteLine("Verifique se o nome do banco está correto!");
                        break;

                    default:
                        Console.WriteLine(erro.GetType());
                        Console.WriteLine(erro.Message);
                        Console.WriteLine(erro.Statement);
                        Console.WriteLine(erro.ToString());
                        break;
                }
            }
            //faz o tratamentos dos erros associados ao acesso ao servidor do banco
            catch (TimeoutException)
            {
                Console.WriteLine("Não foi possível conectar no servidor");
                Console.WriteLine("Verifique se o endereço do servidor está correto!");
                Console.WriteLine();
            }
            //tratamento dos demais erros que possam ocorrer
            catch (Exception erro)
            {
                Console.WriteLine(erro.GetType());
                Console.WriteLine(erro.Message);
                Console.WriteLine(erro.ToString());
            }
        }
    }
}
