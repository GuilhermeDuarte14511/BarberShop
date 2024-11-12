﻿using BarberShop.Application.Interfaces;
using BarberShop.Domain.Entities;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Application.Services
{
    public class PlanoAssinaturaService : IPlanoAssinaturaService
    {
        private readonly IPlanoAssinaturaRepository _planoAssinaturaRepository;

        public PlanoAssinaturaService(IPlanoAssinaturaRepository planoAssinaturaRepository)
        {
            _planoAssinaturaRepository = planoAssinaturaRepository;
        }

        public async Task<List<PlanoAssinaturaSistema>> GetAllPlanosAsync()
        {
            return await _planoAssinaturaRepository.GetAllPlanosAsync();
        }

        public async Task<List<PlanoAssinaturaSistema>> SincronizarPlanosComStripe()
        {
            var service = new ProductService();
            var products = await service.ListAsync(new ProductListOptions
            {
                Active = true,
                Limit = 100
            });

            var planosAtualizados = new List<PlanoAssinaturaSistema>();

            foreach (var product in products)
            {
                var priceService = new PriceService();
                var prices = await priceService.ListAsync(new PriceListOptions
                {
                    Product = product.Id,
                    Limit = 1
                });

                if (prices.Data.Count > 0)
                {
                    var price = prices.Data[0];

                    // Tratamento para o intervalo de cobrança
                    string periodicidade = price.Recurring.Interval;
                    if (periodicidade == "month")
                    {
                        periodicidade = "Mensal";
                    }
                    else if (periodicidade == "year")
                    {
                        periodicidade = "Anual";
                    }

                    // Buscar o plano existente no banco usando o IdProdutoStripe
                    var planoExistente = await _planoAssinaturaRepository.GetByStripeIdAsync(product.Id);

                    if (planoExistente == null)
                    {
                        var novoPlano = new PlanoAssinaturaSistema
                        {
                            Nome = product.Name,
                            Descricao = product.Description,
                            IdProdutoStripe = product.Id,
                            Valor = (decimal)(price.UnitAmount / 100.0), // Converte de centavos para moeda
                            Periodicidade = periodicidade
                        };

                        await _planoAssinaturaRepository.AddAsync(novoPlano);
                        planosAtualizados.Add(novoPlano);
                    }
                    else
                    {
                        // Atualiza o plano existente
                        planoExistente.Nome = product.Name;
                        planoExistente.Descricao = product.Description;
                        planoExistente.IdProdutoStripe = product.Id;
                        planoExistente.Valor = (decimal)(price.UnitAmount / 100.0);
                        planoExistente.Periodicidade = periodicidade;

                        await _planoAssinaturaRepository.UpdateAsync(planoExistente);
                        planosAtualizados.Add(planoExistente);
                    }
                }
            }

            // Salva as alterações no banco
            await _planoAssinaturaRepository.SaveChangesAsync();

            return planosAtualizados;
        }


    }
}
