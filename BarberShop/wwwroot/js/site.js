$(document).ready(function () {
    // Lógica do login
    if ($('#loginPage').length > 0) {
        function isValidEmail(email) {
            var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return regex.test(email);
        }

        function isValidPhone(phone) {
            var regex = /^\(\d{2}\)\s\d{5}-\d{4}$/;
            return regex.test(phone);
        }

        var emailDomains = ["gmail.com", "yahoo.com.br", "outlook.com", "hotmail.com"];

        // Quando o usuário digitar no campo de telefone
        $('#phoneInput').on('input', function () {
            if ($(this).val().length > 0) {
                $('#emailInputContainer').slideUp();
            } else {
                $('#emailInputContainer').slideDown();
            }

            var inputValue = $(this).val().replace(/\D/g, '');
            if (inputValue.length <= 11) {
                if (inputValue.length === 11) {
                    var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                    $(this).val(phoneNumber);
                } else {
                    $(this).val(inputValue);
                }
            }
        });

        // Autocomplete para email
        $('#emailInput').on('input', function () {
            if ($(this).val().length > 0) {
                $('#phoneInputContainer').slideUp();
            } else {
                $('#phoneInputContainer').slideDown();
            }

            var inputValue = $(this).val();
            if (inputValue.includes('@') && inputValue.indexOf('@') === inputValue.length - 1) {
                var dropdownHtml = '';
                emailDomains.forEach(function (domain) {
                    dropdownHtml += '<div class="autocomplete-suggestion">' + inputValue + domain + '</div>';
                });
                $('#emailAutocomplete').html(dropdownHtml).fadeIn();
            } else {
                $('#emailAutocomplete').fadeOut();
            }
        });

        $(document).on('click', '.autocomplete-suggestion', function () {
            var selectedEmail = $(this).text();
            $('#emailInput').val(selectedEmail);
            $('#emailAutocomplete').fadeOut();
        });

        $('#loginForm').on('submit', function (e) {
            var phoneValue = $('#phoneInput').val().trim();
            var emailValue = $('#emailInput').val().trim();

            if ((!isValidEmail(emailValue) && emailValue.length > 0) || (!isValidPhone(phoneValue) && phoneValue.length > 0)) {
                e.preventDefault();
                $('#errorMessage').fadeIn();
            } else {
                $('#errorMessage').fadeOut();
                $('#loadingSpinner').fadeIn();
                $('button[type="submit"]').prop('disabled', true);
            }
        });
    }

    // Exibir o toast de erro de login, se houver
    if ($('#loginErrorToast').length > 0) {
        var toastEl = new bootstrap.Toast(document.getElementById('loginErrorToast'));
        toastEl.show();
    }

    // Lógica do menuPrincipal
    if ($('#menuPrincipal').length > 0) {
        $('#historicoButton, #servicoButton').on('click', function (e) {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
        });
    }

    // Lógica para a página de Solicitar Serviço
    if ($('#solicitarServicoPage').length > 0) {
        var servicosSelecionados = [];
        var valorTotal = 0;
        var duracaoTotal = 0;

        window.adicionarServico = function (id, nome, preco, duracao, element) {
            var index = servicosSelecionados.findIndex(servico => servico.id === id);

            if (index === -1) {
                servicosSelecionados.push({ id, nome, preco, duracao });
                valorTotal += parseFloat(preco);
                duracaoTotal += parseInt(duracao); // Atualiza duração total
                $(element).prop('disabled', true);
            }

            atualizarListaServicosSelecionados();
        };

        window.removerServico = function (index, id) {
            valorTotal -= parseFloat(servicosSelecionados[index].preco);
            duracaoTotal -= parseInt(servicosSelecionados[index].duracao); // Remove duração
            servicosSelecionados.splice(index, 1);

            $('#servico-' + id).prop('disabled', false);
            atualizarListaServicosSelecionados();
        };

        function atualizarListaServicosSelecionados() {
            var lista = $('#servicosSelecionados');
            lista.empty();

            servicosSelecionados.forEach(function (servico, index) {
                lista.append(
                    `<li class="list-group-item d-flex justify-content-between align-items-center">
                        <span>${servico.nome} - R$ ${servico.preco}</span>
                        <button class="btn btn-danger btn-sm" onclick="removerServico(${index}, '${servico.id}')">Remover</button>
                    </li>`
                );
            });

            $('#valorTotal').text(valorTotal.toFixed(2));
        }

        window.confirmarServico = function () {
            if (servicosSelecionados.length === 0) {
                alert('Nenhum serviço selecionado.');
                return;
            }

            var servicoIds = servicosSelecionados.map(s => s.id); // Coletar os IDs dos serviços selecionados

            $('#loadingSpinner').fadeIn();
            setTimeout(function () {
                $('#loadingSpinner').fadeOut();
                // Redirecionar para a escolha do barbeiro com duracaoTotal e servicoIds
                window.location.href = `/Cliente/EscolherBarbeiro?duracaoTotal=${duracaoTotal}&servicoIds=${servicoIds.join(',')}`;
            }, 2000);
        };

        atualizarListaServicosSelecionados();
    }

    // Lógica para a página de Escolher Barbeiro
    if ($('#escolherBarbeiroPage').length > 0) {
        $('.barbeiro-btn').on('click', function () {
            var barbeiroId = $(this).data('barbeiro-id');
            var duracaoTotal = $(this).data('duracao-total');
            var servicoIds = $(this).data('servico-ids'); // Capturar os IDs dos serviços selecionados

            if (!duracaoTotal || duracaoTotal <= 0) {
                alert("Nenhum serviço selecionado ou duração inválida.");
                return;
            }

            $('#calendarioModal').modal('show');
            carregarHorariosDropdown(barbeiroId, duracaoTotal);
        });

        function carregarHorariosDropdown(barbeiroId, duracaoTotal) {
            $('#loadingSpinner').fadeIn();
            $.ajax({
                url: '/Cliente/ObterHorariosDisponiveis',
                data: {
                    barbeiroId: barbeiroId,
                    duracaoTotal: duracaoTotal
                },
                success: function (data) {
                    var select = $('#horariosDisponiveis');
                    select.empty();
                    select.append('<option value="">Escolha um horário...</option>');

                    data.forEach(function (horario) {
                        var diaSemana = dayjs(horario).format('dddd');
                        var dataFormatada = dayjs(horario).format('DD/MM');
                        var horarioFormatado = dayjs(horario).format('HH:mm') + ' - ' + dayjs(horario).add(duracaoTotal, 'minute').format('HH:mm');

                        var optionText = `${diaSemana} (${dataFormatada}) - ${horarioFormatado}`;
                        select.append(`<option value="${horario}">${optionText}</option>`);
                    });

                    $('#loadingSpinner').fadeOut();
                },
                error: function () {
                    alert('Erro ao carregar os horários.');
                    $('#loadingSpinner').fadeOut();
                }
            });
        }

        // Confirmar o horário e redirecionar para a tela de resumo do agendamento
        $('#confirmarHorarioBtn').on('click', function () {
            var horarioSelecionado = $('#horariosDisponiveis').val();
            var barbeiroId = $('.barbeiro-btn').data('barbeiro-id');
            var servicoIds = $('#escolherBarbeiroPage').data('servico-ids'); // Recuperando os IDs dos serviços

            if (!horarioSelecionado) {
                alert('Por favor, selecione um horário.');
            } else {
                $('#loadingSpinner').fadeIn();
                setTimeout(function () {
                    $('#loadingSpinner').fadeOut();
                    var dataHora = new Date(horarioSelecionado);
                    // Redirecionar para a página de resumo com os parâmetros necessários
                    window.location.href = `/Cliente/ResumoAgendamento?barbeiroId=${barbeiroId}&dataHora=${encodeURIComponent(dataHora.toISOString())}&servicoIds=${servicoIds}`;
                }, 2000);
            }
        });
    }

    // Lógica do resumo de agendamento
    if ($('#resumoAgendamentoPage').length > 0) {
        $('#confirmarAgendamentoBtn').on('click', function () {
            var barbeiroId = $('#resumoAgendamentoPage').data('barbeiro-id');
            var servicoIdsString = $('#resumoAgendamentoPage').data('servico-ids');
            var dataHora = $('#resumoAgendamentoPage').data('data-hora');

            // Converter a string '1,2,3' para um array [1, 2, 3]
            var servicoIds = servicoIdsString.split(',').map(function (id) {
                return parseInt(id, 10); // Convertendo cada ID para número
            });

            // Exibe o spinner de carregamento
            $('#loadingSpinner').fadeIn();

            // Simula a chamada ao servidor ou processo de confirmação
            setTimeout(function () {
                $('#loadingSpinner').fadeOut();

                // Enviar o agendamento via AJAX para o backend
                $.ajax({
                    type: 'POST',
                    url: '/Cliente/ConfirmarAgendamento',
                    data: {
                        barbeiroId: barbeiroId,
                        servicoIds: servicoIds.join(','), // Enviar como string separada por vírgulas
                        dataHora: dataHora
                    },
                    success: function () {
                        // Exibe o modal de sucesso
                        $('#successModal').modal('show');
                    },
                    error: function () {
                        alert('Erro ao confirmar o agendamento.');
                    }
                });
            }, 2000);
        });

        // Redirecionar para o menu principal ao clicar em "OK"
        $('#redirectMenuBtn').on('click', function () {
            window.location.href = '/Cliente/MenuPrincipal';
        });
    }
});
