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
            $('#registerErrorMessage').text("Por favor, preencha todos os campos corretamente.").fadeIn();
            return;
        }

        $('#loadingSpinner').fadeIn();
        $('#registerErrorMessage').fadeOut();

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
                    $('#registerErrorMessage').text(data.message).fadeIn();
                }
            },
            error: function () {
                $('#loadingSpinner').fadeOut();
                $('#registerErrorMessage').text('Erro ao registrar. Por favor, tente novamente.').fadeIn();
            }
        });
    });

    // Submissão do formulário de verificação de código via AJAX
    $('#verificationForm').on('submit', function (e) {
        e.preventDefault();

        var clienteId = $('#clienteIdField').val();
        console.log("Cliente ID sendo enviado para verificação:", clienteId);

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

        var clienteId = $('#clienteIdField').val();
        console.log("Cliente ID para reenvio de código:", clienteId);

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
                $('#codeErrorMessage').text('Erro ao reenviar o código. Por favor, tente novamente.').fadeIn();
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
                alert('Nenhum serviço selecionado.');
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

        $('#confirmarHorarioBtn').on('click', function () {
            var horarioSelecionado = $('#horariosDisponiveis').val();

            if (!horarioSelecionado) {
                alert('Por favor, selecione um horário.');
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
                    $('#successModal').modal('show');
                },
                error: function () {
                    $('#loadingSpinner').fadeOut();
                    alert('Erro ao confirmar o agendamento.');
                }
            });
        });

        $('#redirectMenuBtn').on('click', function () {
            window.location.href = '/Cliente/MenuPrincipal';
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
});
