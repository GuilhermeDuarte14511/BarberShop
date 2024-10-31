$(document).ready(function () {
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

    // Função para exibir Toast
function showToast(message, type) {
    // Define a cor de fundo com base no tipo de mensagem
    let backgroundColor;
    switch (type) {
        case 'success':
            backgroundColor = '#27ae60'; // Verde para sucesso
            break;
        case 'error':
            backgroundColor = '#e74c3c'; // Vermelho para erro
            break;
        case 'info':
            backgroundColor = '#3498db'; // Azul para informativo
            break;
        case 'warning':
            backgroundColor = '#f39c12'; // Amarelo para aviso
            break;
        default:
            backgroundColor = '#2b2b2b'; // Fundo escuro padrão
    }

    var toastEl = $(`
        <div class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true" style="background-color: ${backgroundColor}; color: #ecf0f1; border-radius: 8px; box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.2);">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `);

    $('#toastContainer').append(toastEl);
    var toast = new bootstrap.Toast(toastEl[0], { delay: 5000 }); // Define o tempo de exibição para 5 segundos
    toast.show();

    // Remove o toast da DOM após escondê-lo
    toastEl.on('hidden.bs.toast', function () {
        $(this).remove();
    });

    // Fecha automaticamente o toast após 5 segundos caso o usuário não feche manualmente
    setTimeout(() => {
        toast.hide();
    }, 5000);
}


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
                showToast('Por favor, preencha um email ou telefone válido.', 'danger');
                return;
            } else {
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
                        console.log("Cliente ID obtido após login:", data.clienteId);

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
            showToast("Por favor, preencha todos os campos corretamente.", "danger");
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
                    console.log("Cliente ID obtido após cadastro:", data.clienteId);

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
                    showToast(data.message, "danger");
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
                    clearInterval(countdownInterval);
                    window.location.href = data.redirectUrl;
                } else {
                    showToast(data.message, "danger");
                }
            },
            error: function () {
                showToast('Ocorreu um erro. Por favor, tente novamente.', 'danger');
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
                    showToast('Um novo código foi enviado para o seu email.', "success");
                    resetCountdown();
                } else {
                    showToast(data.message, "danger");
                }
            },
            error: function () {
                showToast('Erro ao reenviar o código. Por favor, tente novamente.', "danger");
            }
        });
    });

    // Funções do contador
    function startCountdown() {
        var timeLeft = countdownTime;
        $('#countdownTimer').text(timeLeft + ' segundos');
        $('#resendCodeLink').hide();

        countdownInterval = setInterval(function () {
            timeLeft--;
            $('#countdownTimer').text(timeLeft + ' segundos');

            if (timeLeft <= 0) {
                clearInterval(countdownInterval);
                $('#countdownTimer').text('O tempo expirou.');
                $('#resendCodeLink').show();
            }
        }, 1000);
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
                showToast('Nenhum serviço selecionado.', "danger");
                return;
            }

            var servicoIds = servicosSelecionados.map(s => s.id);

            $('#loadingSpinner').fadeIn();

            sessionStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));

            window.location.href = `/Barbeiro/EscolherBarbeiro?duracaoTotal=${duracaoTotal}&servicoIds=${servicoIds.join(',')}`;
        };

        var servicosArmazenados = JSON.parse(sessionStorage.getItem('servicosSelecionados')) || [];
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
                showToast("Nenhum serviço selecionado ou duração inválida.", "danger");
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
                    showToast('Erro ao carregar os horários.', "danger");
                    $('#loadingSpinner').fadeOut();
                }
            });
        }

        $('#confirmarHorarioBtn').on('click', function () {
            var horarioSelecionado = $('#horariosDisponiveis').val();

            if (!horarioSelecionado) {
                showToast('Por favor, selecione um horário.', "danger");
            } else {
                $('#loadingSpinner').fadeIn();

                sessionStorage.removeItem('servicosSelecionados');

                var dataHora = new Date(horarioSelecionado);
                window.location.href = `/Agendamento/ResumoAgendamento?barbeiroId=${selectedBarbeiroId}&dataHora=${encodeURIComponent(dataHora.toISOString())}&servicoIds=${selectedServicoIds}`;
            }
        });

        $('#voltarBtn').on('click', function () {
            window.location.href = '/Cliente/SolicitarServico';
        });
    }
if ($('#resumoAgendamentoPage').length > 0) {
    const totalAmount = parseFloat($('#precoTotal').data('preco-total'));
    const paymentContainerId = "paymentBrick_container";
    let cardPaymentBrickController = null;
    const clienteNome = $('#clienteNome').val() || "Cliente";
    const clienteEmail = $('#clienteEmail').val();

    console.log("Cliente Nome:", clienteNome);
    console.log("Cliente Email:", clienteEmail);
    console.log("Total Amount:", totalAmount);

    // Função para inicializar o Payment Brick usando o preferenceId do backend
    async function initializePaymentBrick() {
        try {
            console.log("Iniciando criação da preferência de pagamento...");

            const response = await fetch('/api/payment/create-preference', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    amount: totalAmount,
                    description: "Serviço de Barbearia"
                })
            });

            if (!response.ok) throw new Error(`Erro ao criar preferência: ${response.statusText}`);

            const data = await response.json();
            console.log("Preference ID recebido:", data.preferenceId);

            const bricksBuilder = mp.bricks();

            // Se o brick já existir, desmonta-o
            if (cardPaymentBrickController) {
                cardPaymentBrickController.unmount();
            }

            cardPaymentBrickController = await bricksBuilder.create('cardPayment', paymentContainerId, {
                initialization: {
                    amount: totalAmount,
                    preferenceId: data.preferenceId,
                    payer: { email: clienteEmail, firstName: clienteNome }
                },
                customization: {
                    paymentMethods: {
                        ticket: "all",
                        bankTransfer: "all",
                        creditCard: "all",
                        debitCard: "all",
                        mercadoPago: "all"
                    }
                },
                callbacks: {
                    onReady: () => { console.log("Brick carregado com sucesso!"); },
                    onSubmit: async (formData) => {
                        console.log("Dados de pagamento recebidos no submit:", formData);

                        if (!formData.payer || !formData.payer.first_name) {
                            formData.payer = formData.payer || {};
                            formData.payer.first_name = clienteNome;
                        }

                        return new Promise((resolve, reject) => {
                            $.ajax({
                                type: 'POST',
                                url: '/api/payment/process_payment',
                                data: JSON.stringify({
                                    transactionAmount: formData.transaction_amount,
                                    token: formData.token,
                                    description: "Serviço de Barbearia",
                                    installments: formData.installments,
                                    paymentMethodId: formData.payment_method_id,
                                    payer: {
                                        email: formData.payer.email,
                                        firstName: formData.payer.first_name,
                                        identification: {
                                            type: formData.payer.identification.type,
                                            number: formData.payer.identification.number
                                        }
                                    }
                                }),
                                contentType: 'application/json',
                                success: function (response) {
                                    console.log("Resposta do backend para pagamento:", response);
                                    if (response.status === "approved") {
                                        $('#loadingSpinner').fadeIn();
                                        confirmarAgendamento(response.paymentId); // Passa o paymentId para o backend
                                        $(`#${paymentContainerId} button`).css('background-color', 'green'); // Muda o botão para verde
                                        resolve(); // Confirma que o pagamento foi bem-sucedido
                                    } else {
                                        showToast("Erro ao processar pagamento.", "error");
                                        $(`#${paymentContainerId} button`).css('background-color', 'red'); // Muda o botão para vermelho
                                        reject(); // Rejeita o pagamento
                                    }
                                },
                                error: function (xhr, status, error) {
                                    console.error("Erro ao processar pagamento:", xhr.responseText);
                                    showToast("Erro ao processar pagamento. Tente novamente.", "error");
                                    $(`#${paymentContainerId} button`).css('background-color', 'red'); // Muda o botão para vermelho
                                    reject();
                                }
                            });
                        });
                    },
                    onError: (error) => {
                        console.log("Erro ao carregar método de pagamento:", error);
                        showToast("Erro na inicialização do método de pagamento.", "error");
                        $(`#${paymentContainerId} button`).css('background-color', 'red'); // Muda o botão para vermelho em caso de erro
                    }
                }
            });

            $('#mudarFormaPagamentoBtn').show();
        } catch (error) {
            console.log("Erro na inicialização do Payment Brick:", error);
            showToast("Erro ao inicializar o método de pagamento.", "error");
        }
    }

    // Função para confirmar o agendamento após o pagamento aprovado
    async function confirmarAgendamento(paymentId) {
        const data = {
            barbeiroId: $('#resumoAgendamentoPage').data('barbeiro-id'),
            dataHora: $('#resumoAgendamentoPage').data('data-hora'),
            servicoIds: $('#resumoAgendamentoPage').data('servico-ids'),
            formaPagamento: 'creditCard',
            paymentMethodId: 'mercadopago',
            paymentId: paymentId // Passa o paymentId do pagamento aprovado
        };

        console.log("Enviando confirmação de agendamento com dados:", data);

        $.ajax({
            type: 'POST',
            url: '/Agendamento/ConfirmarAgendamento',
            data: data,
            success: function (response) {
                console.log("Resposta da confirmação de agendamento:", response);
                $('#loadingSpinner').hide();
                $('#successModal').modal('show'); // Exibe o modal de sucesso

                // Configura o evento para redirecionar ao clicar em "OK"
                $('#successModal').on('hidden.bs.modal', function () {
                    window.location.href = "/Cliente/MenuPrincipal";
                });
            },
            error: function (xhr, status, error) {
                console.error("Erro ao confirmar agendamento:", xhr.responseText);
                $('#loadingSpinner').hide();
                showToast("Erro ao confirmar o agendamento. Tente novamente.", "error");
            }
        });
    }

    // Clique no botão de pagamento com Cartão/Pix
    $('#payWithCardOrPixButton').on('click', function () {
        console.log("Selecionado: Pagar com Cartão/Pix");
        $(this).addClass('selected');
        $('#payAtLocationButton').removeClass('selected').hide();
        $('#paymentBrick_container').show();
        $('#paymentButtonsContainer').show();
        $('#confirmarAgendamentoBtn').hide();
        initializePaymentBrick();
    });

    // Clique no botão de pagamento na loja
    $('#payAtLocationButton').on('click', function () {
        console.log("Selecionado: Pagar na Loja");
        $(this).addClass('selected');
        $('#payWithCardOrPixButton').removeClass('selected').hide();
        $('#paymentBrick_container').hide();
        $('#paymentButtonsContainer').show();
        $('#confirmarAgendamentoBtn').show();
    });

    // Clique no botão de Mudar Forma de Pagamento
    $('#mudarFormaPagamentoBtn').on('click', function () {
        console.log("Mudando forma de pagamento");
        $('#paymentButtonsContainer').hide();
        $('#payWithCardOrPixButton').removeClass('selected').show();
        $('#payAtLocationButton').removeClass('selected').show();
        $('#paymentBrick_container').hide();
        if (cardPaymentBrickController) {
            cardPaymentBrickController.unmount();
            cardPaymentBrickController = null;
        }
    });
}



    if ($('#barbeiroPage').length > 0) {
        function aplicarMascaraTelefone(input) {
            input.on('input', function () {
                var inputValue = $(this).val().replace(/\D/g, '');
                if (inputValue.length === 11) {
                    var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                    $(this).val(phoneNumber);
                }
            });
        }

        aplicarMascaraTelefone($('#adicionarTelefone'));
        aplicarMascaraTelefone($('#editarTelefone'));

        function exibirNotificacaoModal(mensagem, tipo) {
            showToast(mensagem, tipo);
        }

        $('#btnAdicionarBarbeiro').on('click', function () {
            $('#adicionarNome').val('');
            $('#adicionarEmail').val('');
            $('#adicionarTelefone').val('');
            $('#adicionarModal').modal('show');
        });

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
                    showToast(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    showToast('Erro ao adicionar o barbeiro.', 'danger');
                }
            });
        });

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
                    showToast(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    showToast('Erro ao editar o barbeiro.', 'danger');
                }
            });
        });

        $('.btnExcluir').on('click', function () {
            var barbeiroId = $(this).data('id');
            var barbeiroNome = $(this).closest('tr').length > 0 ?
                $(this).closest('tr').find('td:first').text() :
                $(this).closest('.barbeiro-card').find('p').first().text().replace('Nome:', '').trim();

            $('#excluirBarbeiroNome').text(barbeiroNome);
            $('#btnConfirmarExcluir').data('id', barbeiroId);
            $('#excluirModal').modal('show');
        });

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
                    showToast(response.message, response.success ? 'success' : 'danger');
                },
                error: function () {
                    $('#loadingSpinner').hide();
                    showToast('Erro ao excluir o barbeiro.', 'danger');
                }
            });
        });
    }

    if ($('#adminDashboard').length > 0) {
            initCharts();
        }

function initCharts() {
    // Gráfico de Agendamentos da Semana
    const agendamentosSemanaChart = new Chart(document.getElementById('agendamentosSemanaChart').getContext('2d'), {
        type: 'bar',
        data: {
            labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
            datasets: [{
                label: 'Agendamentos',
                data: [5, 8, 3, 6, 7, 10, 2],
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Gráfico de Serviços Mais Solicitados
    const servicosMaisSolicitadosChart = new Chart(document.getElementById('servicosMaisSolicitadosChart').getContext('2d'), {
        type: 'pie',
        data: {
            labels: ['Corte de Cabelo', 'Barba', 'Hidratação Capilar'],
            datasets: [{
                data: [12, 8, 5],
                backgroundColor: ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)'],
                borderColor: ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true
        }
    });

    // Gráfico de Lucro por Barbeiro
    const lucroPorBarbeiroChart = new Chart(document.getElementById('lucroPorBarbeiroChart').getContext('2d'), {
        type: 'bar',
        data: {
            labels: ['Rafael Souza', 'Thiago Ribeiro', 'Gustavo Martins', 'Leonardo Costa', 'Bruno Fernandes', 'Carol Momo'],
            datasets: [{
                label: 'Lucro (R$)',
                data: [500, 700, 400, 600, 750, 300],
                backgroundColor: 'rgba(75, 192, 192, 0.5)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Gráfico de Atendimentos por Barbeiro
    const atendimentosPorBarbeiroChart = new Chart(document.getElementById('atendimentosPorBarbeiroChart').getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: ['Rafael Souza', 'Thiago Ribeiro', 'Gustavo Martins', 'Leonardo Costa', 'Bruno Fernandes', 'Carol Momo'],
            datasets: [{
                data: [5, 7, 4, 6, 8, 3],
                backgroundColor: ['rgba(255, 99, 132, 0.5)', 'rgba(54, 162, 235, 0.5)', 'rgba(255, 206, 86, 0.5)', 'rgba(75, 192, 192, 0.5)', 'rgba(153, 102, 255, 0.5)', 'rgba(255, 159, 64, 0.5)'],
                borderColor: ['rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)', 'rgba(75, 192, 192, 1)', 'rgba(153, 102, 255, 1)', 'rgba(255, 159, 64, 1)'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true
        }
    });

    // Gráfico de Lucro da Semana
    const lucroSemanaChart = new Chart(document.getElementById('lucroSemanaChart').getContext('2d'), {
        type: 'line',
        data: {
            labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
            datasets: [{
                label: 'Lucro em R$',
                data: [200, 300, 250, 400, 350, 500, 450],
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 2,
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Gráfico de Lucro do Mês
    const lucroMesChart = new Chart(document.getElementById('lucroMesChart').getContext('2d'), {
        type: 'line',
        data: {
            labels: ['Semana 1', 'Semana 2', 'Semana 3', 'Semana 4'],
            datasets: [{
                label: 'Lucro em R$',
                data: [1500, 2000, 1800, 2200],
                backgroundColor: 'rgba(153, 102, 255, 0.2)',
                borderColor: 'rgba(153, 102, 255, 1)',
                borderWidth: 2,
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
});
