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

                    //exclusão dos registros de 14 ou mais dias atras.
                    banco.Edicao(banco.tabelaCheckin, banco.modoDelete, "", "", "now() - interval '14 days' >= data", conexao);


                    Console.WriteLine("TESTES DOS MÉTODOS: \n");

                    /*vals.Add("11296543760");
                    vals.Add("ola amiga");
                    vals.Add("I aint the sharpest tool");
                    vals.Add("15");
                    vals.Add("in the shed");
                    vals.Add("93026402");
                    vals.Add("");
                    vals.Add("Campinas");
                    vals.Add("SP");

                    banco.Cadastro(banco.tabelaUsuario, vals, conexao);

                    vals.Clear();

                    vals.Add("9573083084739");
                    vals.Add("uoooo");
                    vals.Add("yeeee");
                    vals.Add("435");
                    vals.Add("jejeje");
                    vals.Add("13009282");
                    vals.Add("");
                    vals.Add("Campinas");
                    vals.Add("SP");

                    banco.Cadastro(banco.tabelaEstabelecimento, vals, conexao);

                    vals.Clear();

                    vals.Add("11121314151");
                    vals.Add("17936281908");

                    banco.Telefone(banco.telEstabelecimento, banco.GetCNPJs(conexao)[0], vals, conexao);

                    vals.Clear();

                    var cpfs = banco.GetCPFs(conexao);
                    var cnpjs = banco.GetCNPJs(conexao);

                    vals.Add(cnpjs[1]);
                    vals.Add("2021-01-13");

                    banco.Checkin(cpfs[1], vals, conexao);

                    vals.Clear();*/

                    foreach(KeyValuePair<string,string> v in banco.ChecaRisco(conexao)){
                        Console.WriteLine("CPF do usuario em risco: {0} Nome do local contaminado: {1}", v.Key, v.Value);
                    }


                    /*var ccin = banco.GetCodCheckins(conexao);

                    vals.Add(cpfs[0]);
                    vals.Add("2021-01-15");
                    if(ccin.ContainsKey(cpfs[0])){
                        vals.Add(ccin[cpfs[0]].ToString());
                        banco.Reportar(vals, conexao);
                    }*/
                    

                    //banco.Edicao(banco.tabelaUsuario, banco.modoUpdate, "nome", "novonome", "cpf = '11111111111'", conexao);
                    //banco.Edicao("checkin", banco.modoDelete, "", "", "now() + interval \'14 days\' >= data", conexao);

                    var listg = banco.Listagem(banco.tabelaUsuario, conexao);
                    

                    for(int i = 0;i<listg[0].Count;i++){
                        Console.WriteLine("CPF: " + listg[0][i]);
                        Console.WriteLine("nome: " + listg[1][i]);
                        Console.WriteLine("rua: " + listg[2][i]);
                        Console.WriteLine("n: " + listg[3][i]);
                        Console.WriteLine("bairro: " + listg[4][i]);
                        Console.WriteLine("Cep: " + listg[5][i]);
                        Console.WriteLine("Complemento: " + listg[6][i]);
                        Console.WriteLine("Cidade: " + listg[7][i]);
                        Console.WriteLine("estado: " + listg[8][i]);
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
