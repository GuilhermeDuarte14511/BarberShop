﻿$(document).ready(function () {



    // Função para exibir Toasts
    function showToast(message, type = 'info') {
        const toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

        $('#toastContainer').append(toastHtml);
        const toastElement = $('#toastContainer .toast').last();
        const toastInstance = new bootstrap.Toast(toastElement[0], { delay: 5000 });
        toastInstance.show();

        // Fecha automaticamente após 5 segundos se o usuário não fechar manualmente
        setTimeout(() => {
            toastInstance.hide();
        }, 5000);

        // Remove o toast do DOM após ele ser ocultado
        toastElement.on('hidden.bs.toast', function () {
            $(this).remove();
        });
    }
    // Variáveis globais

    var countdownInterval;
    var countdownTime = 30; // Tempo em segundos

    function isValidEmail(email) {
        var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    function isValidPhone(phone) {
        var regex = /^\(\d{2}\)\s\d{5}-\d{4}$/;
        return regex.test(phone);
    }

    var emailDomains = ["gmail.com", "yahoo.com.br", "outlook.com", "hotmail.com"];

    // Lógica do login
    if ($('#loginPage').length > 0) {
        $('#phoneInput').on('input', function () {
            if ($(this).val().length > 0) {
                $('#emailInputContainer').slideUp();
            } else {
                $('#emailInputContainer').slideDown();
            }

            var inputValue = $(this).val().replace(/\D/g, '');
            if (inputValue.length === 11) {
                var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                $(this).val(phoneNumber);
            }
        });

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
            $('#emailInput').val($(this).text());
            $('#emailAutocomplete').fadeOut();
        });

        // Submissão do formulário de login via AJAX
        $('#loginForm').on('submit', function (e) {
            e.preventDefault();

            var phoneValue = $('#phoneInput').val().trim();
            var emailValue = $('#emailInput').val().trim();

            if ((!isValidEmail(emailValue) && emailValue.length > 0) && (!isValidPhone(phoneValue) && phoneValue.length > 0)) {
                showToast('Email ou telefone inválido.', 'danger');
                return;
            }

            $('#loadingSpinner').fadeIn();
            $('button[type="submit"]').prop('disabled', true);
            var formData = $(this).serialize();

            $.ajax({
                type: 'POST',
                url: '/Login/Login',
                data: formData,
                success: function (data) {
                    $('#loadingSpinner').fadeOut();
                    if (data.success) {
                        $('#clienteIdField').remove();
                        $('<input>').attr({
                            type: 'hidden',
                            id: 'clienteIdField',
                            name: 'clienteId',
                            value: data.clienteId
                        }).appendTo('#verificationForm');
                        $('button[type="submit"]').prop('disabled', false);
                        $('#verificationModal').modal('show');
                        startCountdown();
                    } else {
                        $('button[type="submit"]').prop('disabled', false);
                        showToast(data.message, 'danger');
                    }
                },
                error: function () {
                    $('#loadingSpinner').fadeOut();
                    $('button[type="submit"]').prop('disabled', false);
                    showToast('Ocorreu um erro. Por favor, tente novamente.', 'danger');
                }
            });
        });
    }





    // Funções exclusivas para o modal de cadastro
    $('#registerPhoneInput').on('input', function () {
        var inputValue = $(this).val().replace(/\D/g, '');
        if (inputValue.length === 11) {
            var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
            $(this).val(phoneNumber);
        }
    });

    $('#registerEmailInput').on('input', function () {
        var inputValue = $(this).val();
        if (inputValue.includes('@') && inputValue.indexOf('@') === inputValue.length - 1) {
            var dropdownHtml = '';
            emailDomains.forEach(function (domain) {
                dropdownHtml += '<div class="autocomplete-suggestion">' + inputValue + domain + '</div>';
            });
            $('#registerEmailAutocomplete').html(dropdownHtml).fadeIn();
        } else {
            $('#registerEmailAutocomplete').fadeOut();
        }
    });

    $(document).on('click', '.autocomplete-suggestion', function () {
        $('#registerEmailInput').val($(this).text());
        $('#registerEmailAutocomplete').fadeOut();
    });

    // Submissão do formulário de cadastro via AJAX com validação
    $('#registerForm').on('submit', function (e) {
        e.preventDefault();

        var name = $('#nameInput').val().trim();
        var email = $('#registerEmailInput').val().trim();
        var phone = $('#registerPhoneInput').val().trim();

        if (name === "" || !isValidEmail(email) || !isValidPhone(phone)) {
            showToast("Por favor, preencha todos os campos corretamente.", 'danger');
            return;
        }

        $('#loadingSpinner').fadeIn();
        var formData = $(this).serialize();

        $.ajax({
            type: 'POST',
            url: '/Login/Cadastro',
            data: formData,
            success: function (data) {
                $('#loadingSpinner').fadeOut();
                if (data.success) {
                    $('#clienteIdField').remove();
                    $('<input>').attr({
                        type: 'hidden',
                        id: 'clienteIdField',
                        name: 'clienteId',
                        value: data.clienteId
                    }).appendTo('#verificationForm');
                    $('#registerModal').modal('hide');
                    $('#verificationModal').modal('show');
                    startCountdown();
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                $('#loadingSpinner').fadeOut();
                showToast('Erro ao registrar. Por favor, tente novamente.', 'danger');
            }
        });
    });

    // Submissão do formulário de verificação de código via AJAX
    $('#verificationForm').on('submit', function (e) {
        e.preventDefault();
        var clienteId = $('#clienteIdField').val();
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
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                showToast('Ocorreu um erro. Tente novamente.', 'danger');
            }
        });
    });

    // Reenvio do código de verificação via AJAX
    $('#resendCode').on('click', function (e) {
        e.preventDefault();
        var clienteId = $('#clienteIdField').val();

        $.ajax({
            type: 'GET',
            url: '/Login/ReenviarCodigo',
            data: { clienteId: clienteId },
            success: function (data) {
                if (data.success) {
                    showToast('Novo código enviado.', 'success');
                    resetCountdown();
                } else {
                    showToast(data.message, 'danger');
                }
            },
            error: function () {
                showToast('Erro ao reenviar o código. Tente novamente.', 'danger');
            }
        });
    });

    // Funções do contador
    function startCountdown() {
        var timeLeft = countdownTime;
        $('#countdownText').html('Você poderá solicitar um novo código em: <span id="countdownTimer">' + timeLeft + '</span> segundos.');
        $('#resendCodeLink').hide();

        countdownInterval = setInterval(function () {
            timeLeft--;
            $('#countdownTimer').text(timeLeft);

            if (timeLeft <= 0) {
                clearInterval(countdownInterval);
                $('#countdownText').text('Clique em reenviar para solicitar um novo código.');
                $('#resendCodeLink').show();
            }
        }, 1000);
    }

    function resetCountdown() {
        clearInterval(countdownInterval);
        startCountdown();
    }

    function resetCountdown() {
        clearInterval(countdownInterval);
        startCountdown();
    }

    if ($('#loginErrorToast').length > 0) {
        var toastEl = new bootstrap.Toast(document.getElementById('loginErrorToast'));
        toastEl.show();
    }

    // Lógica do menuPrincipal
    if ($('#menuPrincipal').length > 0) {
        $('#historicoButton').on('click', function () {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
            window.location.href = '/Agendamento/Historico';
        });

        $('#servicoButton').on('click', function () {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
            window.location.href = '/Cliente/SolicitarServico';
        });
    }

    // Lógica para a página de Solicitar Serviço
    if ($('#solicitarServicoPage').length > 0) {
        var servicosSelecionados = [];
        var valorTotal = 0;
        var duracaoTotal = 0;

        window.adicionarServico = function (id, nome, preco, duracao, element) {
            // Substitui vírgula por ponto no preço antes de converter para número
            preco = preco.replace(',', '.');
            console.log("Adicionando serviço:", nome, "Preço:", preco, "Duração:", duracao);

            var index = servicosSelecionados.findIndex(servico => servico.id === id);

            if (index === -1) {
                servicosSelecionados.push({ id, nome, preco, duracao });
                valorTotal += parseFloat(preco);
                duracaoTotal += parseInt(duracao);
                $(element).prop('disabled', true);

                console.log("Serviço adicionado:", servicosSelecionados);
                console.log("Novo valor total (adicionado):", valorTotal);
            }

            atualizarListaServicosSelecionados();
        };

        window.removerServico = function (index, id) {
            valorTotal -= parseFloat(servicosSelecionados[index].preco.toString().replace(',', '.'));
            duracaoTotal -= parseInt(servicosSelecionados[index].duracao);
            servicosSelecionados.splice(index, 1);

            console.log("Serviço removido:", servicosSelecionados);
            console.log("Novo valor total (removido):", valorTotal);

            $('#servico-' + id).prop('disabled', false);
            atualizarListaServicosSelecionados();
        };

        function atualizarListaServicosSelecionados() {
            var lista = $('#servicosSelecionados');
            lista.empty();

            servicosSelecionados.forEach(function (servico, index) {
                lista.append(
                    `<li class="list-group-item d-flex justify-content-between align-items-center">
                    <span>${servico.nome} - R$ ${parseFloat(servico.preco.replace(',', '.')).toFixed(2)}</span>
                    <button class="btn btn-danger btn-sm" onclick="removerServico(${index}, '${servico.id}')">Remover</button>
                </li>`
                );
            });

            $('#valorTotal').text(valorTotal.toFixed(2));
            console.log("Atualizando valor total exibido:", valorTotal.toFixed(2));
        }

        window.confirmarServico = function () {
            if (servicosSelecionados.length === 0) {
                showToast('Nenhum serviço selecionado.', 'danger');
                return;
            }

            var servicoIds = servicosSelecionados.map(s => s.id);

            console.log("Serviços selecionados para confirmação:", servicosSelecionados);
            console.log("Valor total para confirmação:", valorTotal);

            $('#loadingSpinner').fadeIn();

            sessionStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));

            window.location.href = `/Barbeiro/EscolherBarbeiro?duracaoTotal=${duracaoTotal}&servicoIds=${servicoIds.join(',')}`;
        };

        var servicosArmazenados = JSON.parse(sessionStorage.getItem('servicosSelecionados')) || [];
        servicosArmazenados.forEach(function (servico) {
            servicosSelecionados.push(servico);
            valorTotal += parseFloat(servico.preco.toString().replace(',', '.'));
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
        var horarioSelecionado = null; // Variável para armazenar o horário selecionado

        console.log("Página de Escolher Barbeiro carregada");

        $('.barbeiro-btn').on('click', function () {
            selectedBarbeiroId = $(this).data('barbeiro-id');
            console.log("Barbeiro selecionado com ID:", selectedBarbeiroId);

            if (!selectedDuracaoTotal || selectedDuracaoTotal <= 0) {
                showToast("Nenhum serviço selecionado ou duração inválida.", "danger");
                return;
            }

            $('#calendarioModal').modal('show');
            carregarHorariosDropdown(selectedBarbeiroId, selectedDuracaoTotal);
        });

        function carregarHorariosDropdown(barbeiroId, duracaoTotal) {
            $('#loadingSpinner').fadeIn();
            console.log(`Carregando horários para o barbeiro ID: ${barbeiroId}, duração: ${duracaoTotal}`);

            $.ajax({
                url: '/Agendamento/ObterHorariosDisponiveis',
                data: {
                    barbeiroId: barbeiroId,
                    duracaoTotal: duracaoTotal
                },
                success: function (data) {
                    console.log("Horários recebidos:", data);
                    var select = $('#horariosDisponiveis');
                    select.empty();
                    select.append('<option value="">Escolha um horário...</option>');

                    data.forEach(function (horario) {
                        var diaSemana = dayjs(horario).format('dddd');
                        var dataFormatada = dayjs(horario).format('DD/MM/YYYY');
                        var horarioFormatado = dayjs(horario).format('HH:mm') + ' - ' + dayjs(horario).add(duracaoTotal, 'minute').format('HH:mm');

                        var optionText = `${diaSemana} (${dataFormatada}) - ${horarioFormatado}`;
                        console.log(`Adicionando opção: ${optionText}, valor: ${horario}`);
                        select.append(`<option value="${horario}">${optionText}</option>`);
                    });

                    $('#loadingSpinner').fadeOut();
                },
                error: function () {
                    showToast('Erro ao carregar os horários.', "danger");
                    $('#loadingSpinner').fadeOut();
                }
            });
        }

        // Evento para capturar o valor selecionado do dropdown
        $('#horariosDisponiveis').on('change', function () {
            var horarioUTC = $(this).val();
            // Formata o horário para o formato brasileiro de 24 horas sem subtrair horas
            horarioSelecionado = dayjs(horarioUTC).format('YYYY-MM-DD HH:mm');
            console.log("Horário selecionado no formato 24 horas:", horarioSelecionado);
        });

        $('#confirmarHorarioBtn').on('click', function () {
            if (!horarioSelecionado) { // Usa a variável armazenada
                showToast('Por favor, selecione um horário.', "warning");
            } else {
                $('#loadingSpinner').fadeIn();

                sessionStorage.removeItem('servicosSelecionados');

                // Usa o valor ajustado em `horarioSelecionado` diretamente na URL
                window.location.href = `/Agendamento/ResumoAgendamento?barbeiroId=${selectedBarbeiroId}&dataHora=${encodeURIComponent(horarioSelecionado)}&servicoIds=${selectedServicoIds}`;
            }
        });

        $('#voltarBtn').on('click', function () {
            window.location.href = '/Cliente/SolicitarServico';
        });
    }

    if ($('#resumoAgendamentoPage').length > 0) {
        let selectedPaymentMethod = null;
        const stripe = Stripe("pk_live_51QFMA5Hl3zYZjP9pt96kFgHES5ArjpXgdXa2AXrZr3IXsqpWC9JpHAsLajdeOMCIoCu31wruWj5SLeqbmh9aeVPU003NEkIbJe");
        let elements = null;
        let cardElement = null;
        let clientSecret = null;
        let agendamentoId = null;

        const appearance = {
            theme: 'stripe',
            variables: {
                colorPrimary: '#007bff',
                colorBackground: '#1a1a2e',
                colorText: '#ffffff',
                colorTextSecondary: '#ffffff',
                colorTextPlaceholder: '#ffffff',
                colorDanger: '#ff3860',
                fontFamily: 'Arial, sans-serif',
                spacingUnit: '4px',
                borderRadius: '4px'
            }
        };

        console.log("Página de Resumo de Agendamento carregada");

        async function initializeCardElement() {
            try {
                const amount = parseFloat($('#total-price').data('preco-total'));
                console.log("Inicializando pagamento com valor:", amount);

                const response = await $.ajax({
                    url: '/api/payment/create-payment-intent',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ amount: amount }),
                    dataType: 'json'
                });

                clientSecret = response.clientSecret;
                elements = stripe.elements({ appearance });
                cardElement = elements.create('card');
                cardElement.mount('#payment-element');
                console.log("Elemento de cartão inicializado com clientSecret:", clientSecret);
            } catch (error) {
                console.error('Erro ao inicializar o elemento do cartão:', error);
                showToast('Erro ao configurar o pagamento. Tente novamente mais tarde.', 'danger');
            }
        }

        function selectPaymentMethod(method) {
            selectedPaymentMethod = method;
            $('.payment-option').removeClass('selected').hide();
            $('#' + method + 'Option').addClass('selected').show();
            $('#payment-form, #confirmarAgendamentoBtn').hide();

            if (method === 'creditCard') {
                $('#payment-form').fadeIn();
                initializeCardElement();
            } else if (method === 'store') {
                $('#confirmarAgendamentoBtn').fadeIn();
            }

            $('#changePaymentMethodBtn').show();
        }

        async function confirmarAgendamento() {
            const barbeiroId = $('#resumoAgendamentoPage').data('barbeiro-id');
            const dataHora = $('#resumoAgendamentoPage').data('data-hora');
            const servicoIds = $('#resumoAgendamentoPage').data('servico-ids');

            try {
                const response = await $.post('/Agendamento/ConfirmarAgendamento', {
                    barbeiroId,
                    dataHora,
                    servicoIds,
                    formaPagamento: selectedPaymentMethod
                });

                if (response.success) {
                    agendamentoId = response.agendamentoId;
                    return true;
                } else {
                    showToast(response.message || 'Erro ao confirmar agendamento.', 'danger');
                    $('#errorMessage').text(response.message || 'Erro ao confirmar agendamento. Tente novamente.');
                    $('#errorModal').modal('show');
                    $('#errorRedirectBtn').off('click').on('click', function () {
                        $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                    });
                    return false;
                }
            } catch (error) {
                console.error('Erro ao confirmar agendamento:', error);
                showToast('Erro ao confirmar o agendamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao confirmar o agendamento. Tente novamente mais tarde.');
                $('#errorModal').modal('show');
                $('#errorRedirectBtn').off('click').on('click', function () {
                    $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                });
                return false;
            }
        }

        async function processarPagamento() {
            $('#payment-form').submit();
        }

        $('#payment-form').on('submit', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            try {
                if (selectedPaymentMethod === 'creditCard' && clientSecret) {
                    const clienteNome = $('input[name="clienteNome"]').val();
                    const clienteEmail = $('input[name="clienteEmail"]').val();

                    const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
                        payment_method: {
                            card: cardElement,
                            billing_details: {
                                name: clienteNome,
                                email: clienteEmail
                            }
                        }
                    });

                    if (error) {
                        console.error('Erro no pagamento:', error);
                        showToast('O pagamento não foi concluído. Entre em contato com a loja.', 'danger');
                        $('#errorMessage').text('O pagamento não foi concluído. Entre em contato com a barbearia para confirmar o agendamento.');
                        $('#errorModal').modal('show');
                        $('#errorRedirectBtn').off('click').on('click', function () {
                            window.location.href = '/Cliente/MenuPrincipal';
                        });
                    } else if (paymentIntent && paymentIntent.status === 'succeeded') {
                        await atualizarStatusPagamento(agendamentoId, "Aprovado", paymentIntent.id);
                    } else {
                        console.warn('Pagamento não concluído. Verifique os dados.');
                        showToast('O pagamento não foi concluído. Verifique os dados e tente novamente.', 'warning');
                        $('#errorMessage').text('O pagamento não foi concluído. Verifique os dados e tente novamente.');
                        $('#errorModal').modal('show');
                        $('#errorRedirectBtn').off('click').on('click', function () {
                            $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                        });
                    }
                } else {
                    showToast('Erro: clientSecret não definido.', 'danger');
                    $('#errorMessage').text('Erro: clientSecret não definido.');
                    $('#errorModal').modal('show');
                    $('#errorRedirectBtn').off('click').on('click', function () {
                        $('#errorModal').modal('hide'); // Fechar o modal para tentar novamente
                    });
                }
            } catch (error) {
                console.error('Erro durante a confirmação do pagamento:', error);
                showToast('Erro ao confirmar o pagamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao confirmar o pagamento. Entre em contato com a barbearia para confirmar o agendamento.');
                $('#errorModal').modal('show');
                $('#errorRedirectBtn').off('click').on('click', function () {
                    window.location.href = '/Cliente/MenuPrincipal';
                });
            } finally {
                $('#loadingSpinner').fadeOut();
            }
        });

        async function atualizarStatusPagamento(agendamentoId, statusPagamento, paymentId) {
            try {
                const response = await $.post('/Agendamento/AtualizarStatusPagamento', {
                    agendamentoId,
                    statusPagamento,
                    paymentId
                });

                if (response.success) {
                    showToast('Agendamento e pagamento confirmados com sucesso!', 'success');
                    $('#successModal').modal('show');
                } else {
                    showToast(response.message || 'Erro ao atualizar status do pagamento.', 'danger');
                    $('#errorMessage').text(response.message || 'Erro ao atualizar status do pagamento.');
                    $('#errorModal').modal('show');
                }
            } catch (error) {
                console.error('Erro ao atualizar status do pagamento:', error);
                showToast('Erro ao atualizar o status do pagamento. Tente novamente mais tarde.', 'danger');
                $('#errorMessage').text('Erro ao atualizar o status do pagamento. Tente novamente mais tarde.');
                $('#errorModal').modal('show');
            }
        }

        function redirecionarParaMenu() {
            $('#successModal').on('hidden.bs.modal', function () {
                window.location.href = '/Cliente/MenuPrincipal';
            });
            $('#successModal').modal('show');
        }

        $('#changePaymentMethodBtn').on('click', function () {
            selectedPaymentMethod = null;
            $('#payment-form, #confirmarAgendamentoBtn, #changePaymentMethodBtn').hide();
            $('.payment-option').removeClass('selected').show();

            if (cardElement) {
                cardElement.unmount();
                cardElement = null;
                elements = null;
            }
        });

        $('#creditCardOption').on('click', function () { selectPaymentMethod('creditCard'); });
        $('#storeOption').on('click', function () { selectPaymentMethod('store'); });

        $('#submit').on('click', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            const agendamentoConfirmado = await confirmarAgendamento();

            if (agendamentoConfirmado) {
                showToast('Confirmando pagamento...', 'info');
                await processarPagamento();
            } else {
                showToast("Erro ao confirmar o agendamento. Tente novamente.", 'danger');
            }

            $('#loadingSpinner').fadeOut();
        });

        $('#confirmarAgendamentoBtn').on('click', async function (e) {
            e.preventDefault();
            $('#loadingSpinner').fadeIn();

            const agendamentoConfirmado = await confirmarAgendamento();

            if (agendamentoConfirmado) {
                showToast('Agendamento confirmado com sucesso!', 'success');
                $('#successModal').modal('show');
            } else {
                $('#errorModal').modal('show');
            }

            $('#loadingSpinner').fadeOut();
        });

        $('#redirectMenuBtn').on('click', function () {
            window.location.href = '/Cliente/MenuPrincipal';
        });

        $('#errorRedirectBtn').on('click', function () {
            $('#errorModal').modal('hide');
        });
    }















    if ($('#barbeiroPage').length > 0) {
        // Função para aplicar máscara de telefone
        function aplicarMascaraTelefone(input) {
            input.on('input', function () {
                var inputValue = $(this).val().replace(/\D/g, '');
                if (inputValue.length === 11) {
                    var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                    $(this).val(phoneNumber);
                }
            });
        }

        // Aplicando máscara nos campos de telefone
        aplicarMascaraTelefone($('#adicionarTelefone'));
        aplicarMascaraTelefone($('#editarTelefone'));

        // Função para exibir o modal de notificação
        function exibirNotificacaoModal(mensagem, tipo) {
            $('#notificacaoMensagem').text(mensagem);
            $('#notificacaoModal').modal('show');
            $('#notificacaoModal .modal-body').removeClass('text-success text-danger')
                .addClass(tipo === 'success' ? 'text-success' : 'text-danger');

            // Fecha o modal após 5 segundos e recarrega a página
            setTimeout(function () {
                $('#notificacaoModal').modal('hide');
                location.reload();
            }, 5000);
        }

        // Ação para o botão "Adicionar Barbeiro"
        $('#btnAdicionarBarbeiro').on('click', function () {
            $('#adicionarNome').val('');
            $('#adicionarEmail').val('');
            $('#adicionarTelefone').val('');
            $('#adicionarModal').modal('show');
        });

        // Submissão do formulário de adição via AJAX
        $('#formAdicionarBarbeiro').on('submit', function (e) {
            e.preventDefault();
            var formData = $(this).serialize();
            $('#loadingSpinner').show();

            $.ajax({
                url: '/Barbeiro/Create',
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#loadingSpinner').hide();
                    $('#adicionarModal').modal('hide');
                    exibirNotificacaoModal(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    exibirNotificacaoModal('Erro ao adicionar o barbeiro.', 'danger');
                }
            });
        });

        // Ação para o botão de editar
        $('.btnEditar').on('click', function () {
            var barbeiroId = $(this).data('id');
            $('#loadingSpinner').show();

            $.get(`/Barbeiro/Details/${barbeiroId}`, function (data) {
                $('#loadingSpinner').hide();
                $('#editarBarbeiroId').val(data.barbeiroId);
                $('#editarNome').val(data.nome);
                $('#editarEmail').val(data.email);
                $('#editarTelefone').val(data.telefone);
                $('#editarModal').modal('show');
            });
        });

        // Submissão do formulário de edição via AJAX
        $('#formEditarBarbeiro').on('submit', function (e) {
            e.preventDefault();
            var formData = $(this).serialize();
            var barbeiroId = $('#editarBarbeiroId').val();
            $('#loadingSpinner').show();

            $.ajax({
                url: `/Barbeiro/Edit/${barbeiroId}`,
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#loadingSpinner').hide();
                    $('#editarModal').modal('hide');
                    exibirNotificacaoModal(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    exibirNotificacaoModal('Erro ao editar o barbeiro.', 'danger');
                }
            });
        });

        // Ação para o botão de excluir
        $('.btnExcluir').on('click', function () {
            var barbeiroId = $(this).data('id');

            // Verificar se o botão está em uma tabela ou em um cartão
            var barbeiroNome = $(this).closest('tr').length > 0 ?
                $(this).closest('tr').find('td:first').text() :
                $(this).closest('.barbeiro-card').find('p').first().text().replace('Nome:', '').trim();

            $('#excluirBarbeiroNome').text(barbeiroNome); // Exibe o nome no modal de exclusão
            $('#btnConfirmarExcluir').data('id', barbeiroId);
            $('#excluirModal').modal('show');
        });

        // Confirmação de exclusão
        $('#btnConfirmarExcluir').on('click', function () {
            var barbeiroId = $(this).data('id');
            $('#loadingSpinner').show();

            $.ajax({
                url: '/Barbeiro/DeleteConfirmed',
                type: 'POST',
                data: { id: barbeiroId },
                success: function (response) {
                    $('#loadingSpinner').hide();
                    $('#excluirModal').modal('hide');
                    exibirNotificacaoModal(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    exibirNotificacaoModal('Erro ao excluir o barbeiro.', 'danger');
                }
            });
        });

        // Fecha o modal de notificação ao clicar no botão "OK" e recarrega a página
        $('#notificacaoModal .btn-primary').on('click', function () {
            $('#notificacaoModal').modal('hide');
            location.reload();
        });
    }

    // Lógica do login administrativo
    if ($('#adminLoginPageAdm').length > 0) {
        console.log("Página de login administrativo carregada.");

        const mensagens = [
            "Bem-vindo ao ambiente administrativo da BarberShop",
            "O Ambiente administrativo perfeito para sua barbearia"
        ];
        let indiceMensagemAtual = 0;

        function atualizarMensagem() {
            $('#adminWelcomeTextAdm').fadeOut(1000, function () {
                $(this).text(mensagens[indiceMensagemAtual]).fadeIn(1000);
                indiceMensagemAtual = (indiceMensagemAtual + 1) % mensagens.length;
            });
        }

        setInterval(atualizarMensagem, 7000);

        let tempoContagemRegressivaAdm = 30;
        let intervaloContagemRegressivaAdm;

        function iniciarContagemRegressivaAdm() {
            console.log("Iniciando contagem regressiva...");
            tempoContagemRegressivaAdm = 30;
            $('#adminCountdownTimerAdm').text(tempoContagemRegressivaAdm);
            $('#resendCodeLinkAdm').hide();
            intervaloContagemRegressivaAdm = setInterval(function () {
                tempoContagemRegressivaAdm--;
                $('#adminCountdownTimerAdm').text(tempoContagemRegressivaAdm);
                if (tempoContagemRegressivaAdm <= 0) {
                    clearInterval(intervaloContagemRegressivaAdm);
                    $('#resendCodeLinkAdm').show();
                    console.log("Contagem regressiva finalizada.");
                }
            }, 1000);
        }

        function validarEmailAdm(email) {
            const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return regex.test(email);
        }

        $('#togglePasswordAdm').on('click', function () {
            const campoSenha = $('#adminPasswordInputAdm');
            const icone = $(this).find('i');

            if (campoSenha.attr('type') === 'password') {
                campoSenha.attr('type', 'text');
                icone.removeClass('fa-eye').addClass('fa-eye-slash');
            } else {
                campoSenha.attr('type', 'password');
                icone.removeClass('fa-eye-slash').addClass('fa-eye');
            }
        });

        $('#adminLoginFormAdm').on('submit', function (e) {
            e.preventDefault();
            console.log("Tentativa de login administrativo.");

            const email = $('#adminEmailInputAdm').val().trim();
            const senha = $('#adminPasswordInputAdm').val().trim();

            if (!validarEmailAdm(email)) {
                showToast('Por favor, insira um e-mail válido.', 'danger');
                return;
            }

            if (senha.length === 0) {
                showToast('Por favor, insira sua senha.', 'danger');
                return;
            }

            $('#adminFullScreenSpinnerAdm').fadeIn(); // Mostra o spinner
            $('#adminSubmitButtonAdm').prop('disabled', true);

            const formData = $(this).serialize();
            $.ajax({
                type: 'POST',
                url: '/Login/AdminLogin',
                data: formData,
                success: function (data) {
                    $('#adminFullScreenSpinnerAdm').fadeOut(); // Esconde o spinner após sucesso
                    if (data.success) {
                        console.log("Login administrativo bem-sucedido.");
                        $('#usuarioIdFieldAdm').val(data.usuarioId);
                        $('#adminSubmitButtonAdm').prop('disabled', false);
                        $('#verificationModalAdm').modal('show');
                        iniciarContagemRegressivaAdm();
                    } else {
                        $('#adminSubmitButtonAdm').prop('disabled', false);
                        showToast(data.message, 'danger');
                        console.log("Erro no login administrativo:", data.message);
                    }
                },
                error: function (xhr, status, error) {
                    $('#adminFullScreenSpinnerAdm').fadeOut(); // Esconde o spinner após erro
                    $('#adminSubmitButtonAdm').prop('disabled', false);
                    showToast('Ocorreu um erro. Por favor, tente novamente.', 'danger');
                    console.log("Erro na requisição de login administrativo:", error);
                }
            });
        });

        $('#resendCodeAdm').on('click', function (e) {
            e.preventDefault();
            console.log("Solicitação de reenvio de código.");

            const usuarioId = $('#usuarioIdFieldAdm').val();

            $.ajax({
                type: 'GET',
                url: `/Login/ReenviarCodigoAdm?usuarioId=${usuarioId}`,
                success: function (data) {
                    if (data.success) {
                        showToast("Código de verificação reenviado!", 'info');
                        iniciarContagemRegressivaAdm();
                        console.log("Código de verificação reenviado com sucesso.");
                    } else {
                        showToast(data.message, 'danger');
                        console.log("Erro ao reenviar código:", data.message);
                    }
                },
                error: function (xhr, status, error) {
                    showToast('Erro ao reenviar o código. Tente novamente.', 'danger');
                    console.log("Erro na requisição de reenvio de código:", error);
                }
            });
        });

        $('#verificationFormAdm').on('submit', function (e) {
            e.preventDefault();
            console.log("Tentativa de verificação de código.");

            $('#verifySpinnerAdm').show();
            $('#VerifyCodeAdm').prop('disabled', true);

            const dadosVerificacao = $(this).serialize();
            $.ajax({
                type: 'POST',
                url: '/Login/VerificarAdminCodigo',
                data: dadosVerificacao,
                success: function (data) {
                    $('#verifySpinnerAdm').hide();
                    $('#VerifyCodeAdm').prop('disabled', false);

                    if (data.success) {
                        console.log("Verificação de código bem-sucedida.");
                        window.location.href = data.redirectUrl;
                    } else {
                        $('#codeErrorMessageAdm').show();
                        showToast(data.message, 'danger');
                        console.log("Código inválido ou expirado:", data.message);
                    }
                },
                error: function (xhr, status, error) {
                    $('#verifySpinnerAdm').hide();
                    $('#VerifyCodeAdm').prop('disabled', false);
                    showToast('Erro ao verificar o código. Tente novamente.', 'danger');
                    console.log("Erro na requisição de verificação de código:", error);
                }
            });
        });
    }







    // Variáveis globais para os gráficos
    let agendamentosSemanaChart;
    let servicosMaisSolicitadosChart;
    let lucroPorBarbeiroChart;
    let atendimentosPorBarbeiroChart;
    let lucroSemanaChart;
    let lucroMesChart;

    if ($('#adminDashboard').length > 0) {
        initCharts();
    }

    function initCharts() {
        // Função para destruir o gráfico, se ele já existir
        function destroyChart(chart) {
            if (chart) {
                chart.destroy();
            }
        }

        // Gráfico de Agendamentos da Semana
        destroyChart(agendamentosSemanaChart);
        agendamentosSemanaChart = new Chart(document.getElementById('agendamentosSemanaChart').getContext('2d'), {
            type: 'bar',
            data: {
                labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
                datasets: [{
                    label: 'Agendamentos',
                    data: agendamentosPorSemanaData,  // Usando os dados do servidor
                    backgroundColor: 'rgba(54, 162, 235, 0.5)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, position: 'top', labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Agendamentos: ${context.raw}` } }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { stepSize: 2, font: { size: 12 } }, title: { display: true, text: 'Número de Agendamentos' } },
                    x: { ticks: { font: { size: 12 } } }
                }
            }
        });

        // Gráfico de Serviços Mais Solicitados
        destroyChart(servicosMaisSolicitadosChart);
        servicosMaisSolicitadosChart = new Chart(document.getElementById('servicosMaisSolicitadosChart').getContext('2d'), {
            type: 'pie',
            data: {
                labels: Object.keys(servicosMaisSolicitadosData),
                datasets: [{
                    data: Object.values(servicosMaisSolicitadosData),
                    backgroundColor: ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)'],
                    borderColor: ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)'],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, position: 'bottom', labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Quantidade: ${context.raw}` } }
                }
            }
        });

        // Gráfico de Lucro por Barbeiro
        destroyChart(lucroPorBarbeiroChart);
        lucroPorBarbeiroChart = new Chart(document.getElementById('lucroPorBarbeiroChart').getContext('2d'), {
            type: 'bar',
            data: {
                labels: Object.keys(lucroPorBarbeiroData),
                datasets: [{
                    label: 'Lucro (R$)',
                    data: Object.values(lucroPorBarbeiroData),
                    backgroundColor: 'rgba(75, 192, 192, 0.5)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Lucro: R$${context.raw}` } }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { stepSize: 100, font: { size: 12 } }, title: { display: true, text: 'Lucro em R$' } },
                    x: { ticks: { font: { size: 12 } } }
                }
            }
        });

        // Gráfico de Atendimentos por Barbeiro
        destroyChart(atendimentosPorBarbeiroChart);
        atendimentosPorBarbeiroChart = new Chart(document.getElementById('atendimentosPorBarbeiroChart').getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: Object.keys(atendimentosPorBarbeiroData),
                datasets: [{
                    data: Object.values(atendimentosPorBarbeiroData),
                    backgroundColor: ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)', 'rgba(75, 192, 192, 0.5)', 'rgba(153, 102, 255, 0.5)', 'rgba(255, 159, 64, 0.5)'],
                    borderColor: ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)', 'rgba(75, 192, 192, 1)', 'rgba(153, 102, 255, 1)', 'rgba(255, 159, 64, 1)'],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, position: 'bottom', labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Atendimentos: ${context.raw}` } }
                }
            }
        });

        // Gráfico de Lucro da Semana
        destroyChart(lucroSemanaChart);
        lucroSemanaChart = new Chart(document.getElementById('lucroSemanaChart').getContext('2d'), {
            type: 'line',
            data: {
                labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
                datasets: [{
                    label: 'Lucro em R$',
                    data: lucroDaSemanaData,
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 2,
                    fill: true,
                    pointStyle: 'circle',
                    pointRadius: 5
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Lucro: R$${context.raw}` } }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { stepSize: 100, font: { size: 12 } }, title: { display: true, text: 'Lucro em R$' } },
                    x: { ticks: { font: { size: 12 } } }
                }
            }
        });

        // Gráfico de Lucro do Mês
        destroyChart(lucroMesChart);
        lucroMesChart = new Chart(document.getElementById('lucroMesChart').getContext('2d'), {
            type: 'line',
            data: {
                labels: ['Semana 1', 'Semana 2', 'Semana 3', 'Semana 4'],
                datasets: [{
                    label: 'Lucro em R$',
                    data: lucroDoMesData,
                    backgroundColor: 'rgba(153, 102, 255, 0.2)',
                    borderColor: 'rgba(153, 102, 255, 1)',
                    borderWidth: 2,
                    fill: true,
                    pointStyle: 'rectRot',
                    pointRadius: 6
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { display: true, labels: { font: { size: 14 } } },
                    tooltip: { callbacks: { label: context => `Lucro: R$${context.raw}` } }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { stepSize: 500, font: { size: 12 } }, title: { display: true, text: 'Lucro em R$' } },
                    x: { ticks: { font: { size: 12 } } }
                }
            }
        });
    }



});
