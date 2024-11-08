﻿using System;
using System.Collections.Generic;

namespace BarberShop.Domain.Entities
{
    public class Barbearia
    {
        public int BarbeariaId { get; set; }

        // Nome da barbearia, por exemplo, "CG DREAMS"
        public string Nome { get; set; }

        // Slug único para a URL, por exemplo, "barbeariacgdreams"
        public string UrlSlug { get; set; }

        // Informações de contato
        public string Telefone { get; set; }
        public string Email { get; set; }

        // Endereço completo
        public string Endereco { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }

        // Horário de funcionamento e descrição da barbearia
        public string HorarioFuncionamento { get; set; }
        public string Descricao { get; set; }

        // Status para indicar se a barbearia está ativa (true para ativo, false para inativo)
        public bool Status { get; set; } // Usando bool para refletir o tipo bit no banco

        // Propriedades de navegação
        public ICollection<Barbeiro> Barbeiros { get; set; }
        public ICollection<Servico> Servicos { get; set; }
        public ICollection<Agendamento> Agendamentos { get; set; }

        // Data de criação
        public DateTime DataCriacao { get; set; }

        public byte[] Logo { get; set; } // Nova propriedade para armazenar o logo em formato Base64


        public Barbearia()
        {
            // Inicializa as coleções para evitar nulos
            Barbeiros = new List<Barbeiro>();
            Servicos = new List<Servico>();
            Agendamentos = new List<Agendamento>();
            DataCriacao = DateTime.UtcNow;
            Status = true; // Define a barbearia como ativa por padrão
        }
    }
}