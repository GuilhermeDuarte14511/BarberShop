$(document).ready(function () {



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




    // Verifica se o elemento com o ID 'adminDashboard' está presente na página
    if ($('#adminDashboard').length > 0) {

        // Função para buscar os dados do backend
        async function fetchDashboardData() {
            try {
                const response = await $.ajax({
                    url: "/Dashboard/GetDashboardData",
                    method: "GET",
                });
                return response;
            } catch (error) {
                console.error("Erro ao obter dados do dashboard:", error);
                throw error;
            }
        }

        // Configuração padrão das posições dos gráficos
        const defaultPositions = [
            { GraficoId: "lucroSemanaChart", Posicao: 0 },
            { GraficoId: "lucroMesChart", Posicao: 1 },
            { GraficoId: "agendamentosSemanaChart", Posicao: 2 },
            { GraficoId: "servicosMaisSolicitadosChart", Posicao: 3 },
            { GraficoId: "lucroPorBarbeiroChart", Posicao: 4 },
            { GraficoId: "atendimentosPorBarbeiroChart", Posicao: 5 }
        ];

        // Função para inicializar o dashboard
        async function initializeDashboard() {
            $('#dashboard-container').empty();
            try {
                const data = await fetchDashboardData();
                console.log("Dados recebidos para gráficos:", data);

                if (data && Object.values(data).some(item => item && ((Array.isArray(item) && item.length > 0) || (typeof item === 'object' && Object.keys(item).length > 0)))) {
                    window.dashboardData = data;
                    initializeDashboardCharts(data); // Inicializa os gráficos com os dados recebidos
                    await loadCustomReportsFromLocalStorage(); // Carrega relatórios personalizados salvos no localStorage
                } else {
                    displayDefaultLayout(); // Aplica layout padrão se os dados estiverem vazios
                }
            } catch (error) {
                console.error("Erro ao inicializar o dashboard:", error);
                displayDefaultLayout();
            }
        }

        // Função para exibir um layout padrão se não houver dados
        function displayDefaultLayout() {
            console.log("Exibindo layout padrão - Nenhum dado encontrado.");
            $('#dashboard-container').html(`
        <div class="text-center my-5">
            <h4>Nenhum dado disponível</h4>
            <p>Adicione alguns registros para visualizar os gráficos.</p>
        </div>
    `);
        }

        // Função para renderizar o gráfico com a configuração fornecida
        function renderChart(config, chartId) {
            try {
                $('#dashboard-container').append(`
            <div class="col-lg-4 col-md-6 col-12 dashboard-card" id="${chartId}Card">
                <div class="card">
                    <div class="card-header text-center">
                        <h5>${config.title || "Gráfico"}</h5>
                    </div>
                    <div class="card-body">
                        <canvas id="${chartId}Canvas"></canvas>
                    </div>
                </div>
            </div>
        `);
                initializeChart(`${chartId}Canvas`, config.config); // Usar um ID único para o canvas
            } catch (error) {
                console.error(`Erro ao renderizar o gráfico ${chartId}:`, error);
            }
        }

        // Função para inicializar cada gráfico
        function initializeChart(ctxId, config) {
            const ctx = document.getElementById(ctxId);
            if (ctx) {
                const existingChart = Chart.getChart(ctxId);
                if (existingChart) existingChart.destroy();
                try {
                    new Chart(ctx.getContext('2d'), config);
                } catch (error) {
                    console.error(`Erro ao inicializar o gráfico ${ctxId}:`, error);
                }
            } else {
                console.warn(`Canvas com ID ${ctxId} não encontrado.`);
            }
        }

        // Função para obter a configuração do gráfico pelo ID
        function getChartConfigById(id) {
            const data = window.dashboardData; // Supondo que os dados estejam carregados em uma variável global
            const chartConfigs = {
                lucroSemanaChart: createChartConfig('line', data.lucroDaSemana, 'Lucro da Semana', ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom']),
                lucroMesChart: createChartConfig('line', data.lucroDoMes, 'Lucro do Mês', ['Semana 1', 'Semana 2']),
                agendamentosSemanaChart: createChartConfig('bar', data.agendamentosPorSemana, 'Agendamentos', ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom']),
                servicosMaisSolicitadosChart: createChartConfig('pie', Object.values(data.servicosMaisSolicitados), 'Serviços Mais Solicitados', Object.keys(data.servicosMaisSolicitados)),
                lucroPorBarbeiroChart: createChartConfig('bar', Object.values(data.lucroPorBarbeiro), 'Lucro por Barbeiro', Object.keys(data.lucroPorBarbeiro)),
                atendimentosPorBarbeiroChart: createChartConfig('doughnut', Object.values(data.atendimentosPorBarbeiro), 'Atendimentos por Barbeiro', Object.keys(data.atendimentosPorBarbeiro))
            };

            return chartConfigs[id] || null;
        }

        // Função para criar configurações de gráficos
        function createChartConfig(type, data, label, labels) {
            return {
                type: type,
                data: {
                    labels: labels,
                    datasets: [{
                        label: label,
                        data: data,
                        backgroundColor: generateBackgroundColors(data.length),
                        borderColor: generateBorderColors(data.length),
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: { display: true, labels: { font: { size: 14 } } },
                        tooltip: { callbacks: { label: context => `${label}: ${context.raw}` } }
                    },
                    scales: type !== 'pie' && type !== 'doughnut' ? {
                        y: { beginAtZero: true, title: { display: true, text: label } },
                        x: { ticks: { font: { size: 12 } } }
                    } : {}
                }
            };
        }

        // Função para adicionar relatório personalizado e salvar a chave no localStorage
        async function addCustomReport() {
            const reportType = $('#reportType').val();
            const periodDays = $('#reportPeriod').val();
            const reportKey = `${reportType}-${periodDays}`; // Chave única para cada relatório personalizado

            try {
                const data = await $.ajax({
                    url: `/Dashboard/GetCustomReportData?reportType=${reportType}&periodDays=${periodDays}`,
                    method: "GET"
                });

                console.log("Dados do relatório personalizado:", data);

                const chartConfig = {
                    title: getReportTitle(reportType),
                    config: createChartConfig('bar', Object.values(data), getReportTitle(reportType), Object.keys(data))
                };

                const chartId = `${reportType}CustomChart${Date.now()}`;
                renderChart(chartConfig, chartId);

                // Salva a chave do relatório no localStorage
                saveReportKeyToLocalStorage(reportKey);

                $('#addReportModal').modal('hide');
            } catch (error) {
                console.error("Erro ao adicionar relatório personalizado:", error);
                alert("Erro ao adicionar relatório. Por favor, tente novamente.");
            }
        }

        // Função para salvar uma chave de relatório no localStorage
        function saveReportKeyToLocalStorage(reportKey) {
            let reportKeys = JSON.parse(localStorage.getItem('customReports')) || [];
            if (!reportKeys.includes(reportKey)) {
                reportKeys.push(reportKey);
                localStorage.setItem('customReports', JSON.stringify(reportKeys));
            }
        }

        // Função para carregar relatórios personalizados do localStorage ao iniciar o dashboard
        async function loadCustomReportsFromLocalStorage() {
            const reportKeys = JSON.parse(localStorage.getItem('customReports')) || [];
            for (const reportKey of reportKeys) {
                const [reportType, periodDays] = reportKey.split('-');
                try {
                    const data = await $.ajax({
                        url: `/Dashboard/GetCustomReportData?reportType=${reportType}&periodDays=${periodDays}`,
                        method: "GET"
                    });

                    const chartConfig = {
                        title: getReportTitle(reportType),
                        config: createChartConfig('bar', Object.values(data), getReportTitle(reportType), Object.keys(data))
                    };

                    const chartId = `${reportType}CustomChart${Date.now()}`;
                    renderChart(chartConfig, chartId);
                } catch (error) {
                    console.error(`Erro ao carregar relatório ${reportType}:`, error);
                }
            }
        }


        // Função auxiliar para obter o título do relatório
        function getReportTitle(reportType) {
            const titles = {
                "agendamentosPorStatus": "Agendamentos por Status",
                "servicosMaisSolicitados": "Serviços Mais Solicitados",
                "lucroPorFormaPagamento": "Lucro por Forma de Pagamento",
                "atendimentosPorBarbeiro": "Atendimentos por Barbeiro",
                "clientesFrequentes": "Clientes Frequentes",
                "pagamentosPorStatus": "Pagamentos por Status",
                "servicosPorPreco": "Serviços por Faixa de Preço",
                "lucroPorPeriodo": "Lucro Diário/Semanal/Mensal",
                "tempoMedioPorServico": "Tempo Médio por Tipo de Serviço",
                "agendamentosCancelados": "Agendamentos Cancelados"
            };
            return titles[reportType] || "Relatório Personalizado";
        }

        // Função auxiliar para gerar cores de fundo
        function generateBackgroundColors(count) {
            const colors = ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)', 'rgba(75, 192, 192, 0.5)'];
            return Array.from({ length: count }, (_, i) => colors[i % colors.length]);
        }

        // Função auxiliar para gerar cores de borda
        function generateBorderColors(count) {
            const colors = ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)', 'rgba(75, 192, 192, 1)'];
            return Array.from({ length: count }, (_, i) => colors[i % colors.length]);
        }


        // Função para inicializar todos os gráficos no dashboard
        function initializeDashboardCharts(data) {
            window.dashboardData = data; // Armazena os dados globalmente para uso em restoreInitialPositions
            const chartConfigs = [
                { id: 'lucroSemanaChart', data: data.lucroDaSemana, type: 'line', title: 'Lucro da Semana', labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'] },
                { id: 'lucroMesChart', data: data.lucroDoMes, type: 'line', title: 'Lucro do Mês', labels: ['Semana 1', 'Semana 2'] },
                { id: 'agendamentosSemanaChart', data: data.agendamentosPorSemana, type: 'bar', title: 'Agendamentos', labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'] },
                { id: 'servicosMaisSolicitadosChart', data: Object.values(data.servicosMaisSolicitados), type: 'pie', title: 'Serviços Mais Solicitados', labels: Object.keys(data.servicosMaisSolicitados) },
                { id: 'lucroPorBarbeiroChart', data: Object.values(data.lucroPorBarbeiro), type: 'bar', title: 'Lucro por Barbeiro', labels: Object.keys(data.lucroPorBarbeiro) },
                { id: 'atendimentosPorBarbeiroChart', data: Object.values(data.atendimentosPorBarbeiro), type: 'doughnut', title: 'Atendimentos por Barbeiro', labels: Object.keys(data.atendimentosPorBarbeiro) },
            ];

            chartConfigs.forEach(config => {
                if (config.data && config.data.length > 0) {
                    renderChart({ title: config.title, config: createChartConfig(config.type, config.data, config.title, config.labels) }, config.id);
                }
            });
        }



        // Inicialização do Dashboard ao carregar o documento
        $(document).ready(function () {
            $('#addReportButton').on('click', addCustomReport);
            initializeDashboard();
        });

    }


    if ($('#servicoPage').length > 0) {
        // Função para aplicar máscara de moeda no campo de preço
        function aplicarMascaraPreco(input) {
            input.on('input', function () {
                let valor = $(this).val().replace(/\D/g, ''); // Remove todos os caracteres não numéricos
                valor = (valor / 100).toFixed(2) + ''; // Divide por 100 e fixa duas casas decimais
                valor = valor.replace(".", ","); // Substitui ponto por vírgula
                $(this).val(valor); // Atualiza o valor do campo com a formatação correta
            });
        }

        // Função para converter o valor de preço formatado para decimal antes de enviar para o backend
        function converterPrecoParaDecimal(precoFormatado) {
            return parseFloat(precoFormatado.replace(/\./g, '').replace(',', '.')); // Remove pontos e substitui vírgula por ponto
        }

        // Função para exibir o spinner de carregamento
        function mostrarLoading() {
            $('#loadingSpinnerServico').show();
        }

        // Função para ocultar o spinner de carregamento
        function ocultarLoading() {
            $('#loadingSpinnerServico').hide();
        }

        // Ação para o botão "Adicionar Serviço"
        $('#btnAdicionarServico').on('click', function () {
            $('#adicionarNome').val('');
            $('#adicionarPreco').val('');
            $('#adicionarDuracao').val('');
            $('#adicionarModal').modal('show');
        });

        // Submissão do formulário de adição via AJAX
        $('#formAdicionarServico').on('submit', function (e) {
            e.preventDefault();
            mostrarLoading();

            const formData = {
                Nome: $('#adicionarNome').val(),
                Preco: converterPrecoParaDecimal($('#adicionarPreco').val()), // Converte o preço formatado para decimal
                Duracao: $('#adicionarDuracao').val()
            };

            $.ajax({
                url: '/Servico/Create',
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#adicionarModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500); // Atualiza a página após 1.5 segundos
                    }
                },
                error: function () {
                    showToast('Erro ao adicionar o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Ação para o botão de editar
        $(document).on('click', '.btnEditar', function () {
            const servicoId = $(this).data('id');
            mostrarLoading();

            $.get(`/Servico/Details/${servicoId}`, function (data) {
                $('#editarServicoId').val(data.servicoId);
                $('#editarNome').val(data.nome);
                $('#editarPreco').val(data.preco.toFixed(2).replace('.', ',')); // Aplica a formatação no valor recebido
                $('#editarDuracao').val(data.duracao);
                $('#editarModal').modal('show');
            }).always(function () {
                ocultarLoading();
            });
        });

        // Submissão do formulário de edição via AJAX
        $('#formEditarServico').on('submit', function (e) {
            e.preventDefault();
            mostrarLoading();

            const formData = {
                ServicoId: $('#editarServicoId').val(),
                Nome: $('#editarNome').val(),
                Preco: converterPrecoParaDecimal($('#editarPreco').val()), // Converte o preço formatado para decimal
                Duracao: $('#editarDuracao').val()
            };

            $.ajax({
                url: `/Servico/Edit/${formData.ServicoId}`,
                type: 'POST',
                data: formData,
                success: function (response) {
                    $('#editarModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500); // Atualiza a página após 1.5 segundos
                    }
                },
                error: function () {
                    showToast('Erro ao editar o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Ação para o botão de excluir
        $(document).on('click', '.btnExcluir', function () {
            const servicoId = $(this).data('id');
            const servicoNome = $(this).closest('tr').find('td:first').text();
            $('#excluirServicoNome').text(servicoNome);
            $('#btnConfirmarExcluir').data('id', servicoId);
            $('#excluirModal').modal('show');
        });

        // Confirmação de exclusão
        $('#btnConfirmarExcluir').on('click', function () {
            const servicoId = $(this).data('id');
            mostrarLoading();

            $.ajax({
                url: '/Servico/DeleteConfirmed',
                type: 'POST',
                data: { id: servicoId },
                success: function (response) {
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500); // Atualiza a página após 1.5 segundos
                    }
                },
                error: function () {
                    showToast('Erro ao excluir o serviço.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });
    }


    if ($('#pagamentoPage').length > 0) {

        // Função para exibir o spinner de carregamento
        function mostrarLoading() {
            $('#loadingSpinnerPagamento').show();
        }

        // Função para ocultar o spinner de carregamento
        function ocultarLoading() {
            $('#loadingSpinnerPagamento').hide();
        }

        // Ação para o botão "Ver Detalhes"
        // Ação para o botão "Ver Detalhes"
        $('.btnDetalhes').on('click', function () {
            const pagamentoId = $(this).data('id');
            mostrarLoading();

            // Solicita os detalhes do pagamento via AJAX
            $.get(`/Pagamento/Detalhes/${pagamentoId}`, function (data) {
                console.log(data);
                $('#detalhesModalBody').html(`
            <p><strong>Cliente:</strong> ${data.nomeCliente}</p>
            <p><strong>Valor Pago:</strong> R$ ${data.valorPago ? data.valorPago.toFixed(2).replace('.', ',') : 'N/A'}</p>
            <p><strong>Status:</strong> ${data.statusPagamento}</p>
            <p><strong>Data do Pagamento:</strong> ${data.dataPagamento}</p>
        `);
                $('#detalhesModal').modal('show');
            }).fail(function () {
                showToast('Erro ao carregar os detalhes do pagamento.', 'danger');
            }).always(function () {
                ocultarLoading();
            });
        });


        // Ação para o botão de solicitar reembolso
        $('.btnReembolso').on('click', function () {
            const pagamentoId = $(this).data('id');
            $('#reembolsoPagamentoId').text(pagamentoId);
            $('#btnConfirmarReembolso').data('id', pagamentoId);
            $('#reembolsoModal').modal('show');
        });

        // Confirmação de reembolso via AJAX
        $('#btnConfirmarReembolso').on('click', function () {
            const pagamentoId = $(this).data('id');
            const valorReembolso = $('#inputValorReembolso').val() || null; // valor opcional para reembolso parcial
            mostrarLoading();

            $.ajax({
                url: '/api/payment/refund',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    PaymentId: pagamentoId,
                    Amount: valorReembolso ? parseInt(valorReembolso * 100) : null // converte para centavos, caso haja um valor
                }),
                success: function (response) {
                    $('#reembolsoModal').modal('hide');
                    showToast(response.refundStatus === "succeeded" ? 'Reembolso processado com sucesso.' : 'Reembolso não foi processado.', response.refundStatus === "succeeded" ? 'success' : 'danger');
                    if (response.refundStatus === "succeeded") {
                        setTimeout(() => location.reload(), 1500); // Atualiza a página após 1.5 segundos
                    }
                },
                error: function () {
                    showToast('Erro ao solicitar reembolso.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });

        // Ação para o botão de exclusão
        $('.btnExcluir').on('click', function () {
            const pagamentoId = $(this).data('id');
            const pagamentoNome = $(this).closest('.pagamento-card').find('p:first').text() || $(this).closest('tr').find('td:first').text();
            $('#excluirPagamentoNome').text(pagamentoNome);
            $('#btnConfirmarExcluir').data('id', pagamentoId);
            $('#excluirModal').modal('show');
        });

        // Confirmação de exclusão via AJAX
        $('#btnConfirmarExcluir').on('click', function () {
            const pagamentoId = $(this).data('id');
            mostrarLoading();

            $.ajax({
                url: '/Pagamento/DeleteConfirmed',
                type: 'POST',
                data: { id: pagamentoId },
                success: function (response) {
                    $('#excluirModal').modal('hide');
                    showToast(response.message, response.success ? 'success' : 'danger');
                    if (response.success) {
                        setTimeout(() => location.reload(), 1500); // Atualiza a página após 1.5 segundos
                    }
                },
                error: function () {
                    showToast('Erro ao excluir o pagamento.', 'danger');
                },
                complete: function () {
                    ocultarLoading();
                }
            });
        });
    }


    if ($('#avaliacaoPage').length > 0) {
        console.log('Página de avaliação carregada.');

        // Função para exibir o spinner de carregamento
        function mostrarLoading() {
            $('#avaliacaoLoadingSpinner').show();
            console.log('Spinner de carregamento exibido.');
        }

        // Função para ocultar o spinner de carregamento
        function ocultarLoading() {
            $('#avaliacaoLoadingSpinner').hide();
            console.log('Spinner de carregamento ocultado.');
        }

        // Ação para marcar estrelas ao passar o mouse
        $('#avaliacaoEstrelas .avaliacao-estrela').on('mouseenter', function () {
            $(this).prevAll().addBack().addClass('hover');
            console.log('Mouse entrou em uma estrela:', $(this).data('value'));
        }).on('mouseleave', function () {
            $('#avaliacaoEstrelas .avaliacao-estrela').removeClass('hover');
            console.log('Mouse saiu das estrelas.');
        });

        // Ação para clicar nas estrelas para avaliação
        $('#avaliacaoEstrelas .avaliacao-estrela').on('click', function () {
            const nota = $(this).data('value');
            console.log('Estrela clicada com valor:', nota);
            $('#avaliacaoEstrelas .avaliacao-estrela').removeClass('selecionada');
            $(this).prevAll().addBack().addClass('selecionada');

            // Exibir o campo de observação
            $('#avaliacaoObservacaoContainer').addClass('visible').css({ 'opacity': 1, 'max-height': '150px' });
            console.log('Campo de observação exibido.');
        });

        // Ação para enviar a avaliação
        $('#avaliacaoEnviarBtn').on('click', function () {
            const observacao = $('#avaliacaoObservacao').val();
            const nota = $('#avaliacaoEstrelas .avaliacao-estrela.selecionada').length;

            console.log('Tentando enviar avaliação com nota:', nota);
            if (nota > 0) {
                mostrarLoading();

                // Envio fake da avaliação
                setTimeout(function () {
                    $('#avaliacaoMensagemAgradecimento').fadeIn();
                    console.log('Mensagem de agradecimento exibida.');
                    $('#avaliacaoObservacao').val('');
                    $('#avaliacaoEstrelas .avaliacao-estrela').removeClass('selecionada');
                    $('#avaliacaoObservacaoContainer').removeClass('visible').css({ 'opacity': 0, 'max-height': 0 });

                    setTimeout(function () {
                        $('#avaliacaoMensagemAgradecimento').fadeOut();
                        console.log('Mensagem de agradecimento ocultada.');
                    }, 3000);
                }, 1000);

                ocultarLoading();
            } else {
                alert('Por favor, selecione uma avaliação.');
                console.log('Nenhuma estrela selecionada. Avaliação não enviada.');
            }
        });
    }





});
