﻿$(document).ready(function () {
    // Variáveis globais
    var countdownInterval;
    var countdownTime = 30; // Tempo em segundos

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

        // Submissão do formulário de login via AJAX
        $('#loginForm').on('submit', function (e) {
            e.preventDefault();

            var phoneValue = $('#phoneInput').val().trim();
            var emailValue = $('#emailInput').val().trim();

            if ((!isValidEmail(emailValue) && emailValue.length > 0) && (!isValidPhone(phoneValue) && phoneValue.length > 0)) {
                $('#errorMessage').fadeIn();
                return;
            } else {
                $('#errorMessage').fadeOut();
                $('#loadingSpinner').fadeIn();
                $('button[type="submit"]').prop('disabled', true);
            }

            var formData = $(this).serialize();

            $.ajax({
                type: 'POST',
                url: '/Login/Login',
                data: formData,
                success: function (data) {
                    $('#loadingSpinner').fadeOut();
                    if (data.success) {
                        $('#clienteId').val(data.clienteId);
                        $('button[type="submit"]').prop('disabled', false);
                        $('#verificationModal').modal('show');
                        startCountdown(); // Iniciar o contador de tempo
                    } else {
                        $('button[type="submit"]').prop('disabled', false);
                        $('#errorMessage').text(data.message).fadeIn();
                    }
                },
                error: function () {
                    $('#loadingSpinner').fadeOut();
                    $('button[type="submit"]').prop('disabled', false);
                    $('#errorMessage').text('Ocorreu um erro. Por favor, tente novamente.').fadeIn();
                }
            });
        });

        // Submissão do formulário de verificação de código via AJAX
        $('#verificationForm').on('submit', function (e) {
            e.preventDefault();

            var formData = $(this).serialize();

            $.ajax({
                type: 'POST',
                url: '/Login/VerificarCodigo',
                data: formData,
                success: function (data) {
                    if (data.success) {
                        clearInterval(countdownInterval); // Parar o contador
                        window.location.href = data.redirectUrl;
                    } else {
                        $('#codeErrorMessage').text(data.message).fadeIn();
                    }
                },
                error: function () {
                    $('#codeErrorMessage').text('Ocorreu um erro. Por favor, tente novamente.').fadeIn();
                }
            });
        });

        // Reenvio do código de verificação via AJAX
        $('#resendCode').on('click', function (e) {
            e.preventDefault();

            var clienteId = $('#clienteId').val();

            $.ajax({
                type: 'GET',
                url: '/Login/ReenviarCodigo',
                data: { clienteId: clienteId },
                success: function (data) {
                    if (data.success) {
                        $('#codeErrorMessage').text('Um novo código foi enviado para o seu email.').fadeIn();
                        resetCountdown(); // Reiniciar o contador
                    } else {
                        $('#codeErrorMessage').text(data.message).fadeIn();
                    }
                },
                error: function () {
                    $('#codeErrorMessage').text('Ocorreu um erro ao reenviar o código. Por favor, tente novamente.').fadeIn();
                }
            });
        });

        // Funções do contador
        function startCountdown() {
            var timeLeft = countdownTime;
            $('#countdownTimer').text(timeLeft + ' segundos');
            $('#resendCodeLink').hide();
            $('#codeErrorMessage').hide();

            countdownInterval = setInterval(function () {
                timeLeft--;
                $('#countdownTimer').text(timeLeft + ' segundos');

                if (timeLeft <= 0) {
                    clearInterval(countdownInterval);
                    $('#countdownTimer').text('O tempo expirou.');
                    $('#resendCodeLink').show();
                    // Não desativamos o botão ou o campo de entrada aqui
                }
            }, 1000);
        }

        function resetCountdown() {
            clearInterval(countdownInterval);
            startCountdown();
        }
    }

    // Exibir o toast de erro de login, se houver
    if ($('#loginErrorToast').length > 0) {
        var toastEl = new bootstrap.Toast(document.getElementById('loginErrorToast'));
        toastEl.show();
    }

    // Lógica do menuPrincipal
    if ($('#menuPrincipal').length > 0) {
        $('#historicoButton').on('click', function (e) {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);

            // Redirecionar para o histórico no AgendamentoController
            window.location.href = '/Agendamento/Historico';
        });

        $('#servicoButton').on('click', function (e) {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);

            // Redirecionar para SolicitarServico no ClienteController
            window.location.href = '/Cliente/SolicitarServico';
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
                duracaoTotal += parseInt(duracao);
                $(element).prop('disabled', true);
            }

            atualizarListaServicosSelecionados();
        };

        window.removerServico = function (index, id) {
            valorTotal -= parseFloat(servicosSelecionados[index].preco);
            duracaoTotal -= parseInt(servicosSelecionados[index].duracao);
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

            var servicoIds = servicosSelecionados.map(s => s.id);

            $('#loadingSpinner').fadeIn();

            // Armazenar os serviços selecionados no localStorage
            localStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));

            // Redirecionar para a escolha do barbeiro com duracaoTotal e servicoIds
            window.location.href = `/Barbeiro/EscolherBarbeiro?duracaoTotal=${duracaoTotal}&servicoIds=${servicoIds.join(',')}`;
        };

        // Carregar serviços selecionados do localStorage
        var servicosArmazenados = JSON.parse(localStorage.getItem('servicosSelecionados')) || [];
        servicosArmazenados.forEach(function (servico) {
            servicosSelecionados.push(servico);
            valorTotal += parseFloat(servico.preco);
            duracaoTotal += parseInt(servico.duracao);
            $('#servico-' + servico.id).prop('disabled', true);
        });

        atualizarListaServicosSelecionados();
    }

    // Lógica para a página de Escolher Barbeiro
    if ($('#escolherBarbeiroPage').length > 0) {
        var selectedBarbeiroId = null;
        var selectedDuracaoTotal = $('#escolherBarbeiroPage').data('duracao-total');
        var selectedServicoIds = $('#escolherBarbeiroPage').data('servico-ids');

        $('.barbeiro-btn').on('click', function () {
            selectedBarbeiroId = $(this).data('barbeiro-id');

            if (!selectedDuracaoTotal || selectedDuracaoTotal <= 0) {
                alert("Nenhum serviço selecionado ou duração inválida.");
                return;
            }

            $('#calendarioModal').modal('show');
            carregarHorariosDropdown(selectedBarbeiroId, selectedDuracaoTotal);
        });

        function carregarHorariosDropdown(barbeiroId, duracaoTotal) {
            $('#loadingSpinner').fadeIn();
            $.ajax({
                url: '/Agendamento/ObterHorariosDisponiveis',
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

            if (!horarioSelecionado) {
                alert('Por favor, selecione um horário.');
            } else {
                $('#loadingSpinner').fadeIn();

                // Limpar os serviços selecionados do localStorage
                localStorage.removeItem('servicosSelecionados');

                // Redirecionar para a página de resumo com os parâmetros necessários
                var dataHora = new Date(horarioSelecionado);
                window.location.href = `/Agendamento/ResumoAgendamento?barbeiroId=${selectedBarbeiroId}&dataHora=${encodeURIComponent(dataHora.toISOString())}&servicoIds=${selectedServicoIds}`;
            }
        });

        // Botão de Voltar
        $('#voltarBtn').on('click', function () {
            // Não é necessário passar parâmetros; os serviços estão no localStorage
            window.location.href = '/Cliente/SolicitarServico';
        });
    }

    // Lógica do resumo de agendamento
    if ($('#resumoAgendamentoPage').length > 0) {
        $('#confirmarAgendamentoBtn').on('click', function () {
            var barbeiroId = $('#resumoAgendamentoPage').data('barbeiro-id');
            var servicoIdsString = $('#resumoAgendamentoPage').data('servico-ids');
            var dataHora = $('#resumoAgendamentoPage').data('data-hora');

            $('#loadingSpinner').fadeIn();

            $.ajax({
                type: 'POST',
                url: '/Agendamento/ConfirmarAgendamento',
                data: {
                    barbeiroId: barbeiroId,
                    servicoIds: servicoIdsString,
                    dataHora: dataHora
                },
                success: function () {
                    $('#loadingSpinner').fadeOut();
                    // Exibe o modal de sucesso
                    $('#successModal').modal('show');
                },
                error: function () {
                    $('#loadingSpinner').fadeOut();
                    alert('Erro ao confirmar o agendamento.');
                }
            });
        });

        // Redirecionar para o menu principal ao clicar em "OK"
        $('#redirectMenuBtn').on('click', function () {
            window.location.href = '/Cliente/MenuPrincipal';
        });
    }
});
