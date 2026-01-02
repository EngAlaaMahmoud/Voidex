function createTaxApp(mountSelector) {
    const App = {
        setup() {
                const state = Vue.reactive({
                mainData: [],
                deleteMode: false,
                mainTitle: null,
                id: '',
                mainCode: '',
                subCode: '',
                typeName: '',
                percentage: '',
                description: '',
                errors: {
                    name: '',
                    percentage: '',
                    description: ''
                },
                isSubmitting: false,
            });

            const mainGridRef = Vue.ref(null);
            const mainModalRef = Vue.ref(null);
            const percentageRef = Vue.ref(null);

            const services = {
                getMainData: async () => {
                    try {
                        const response = await AxiosManager.get('/Tax/GetTaxList', {});
                        return response;
                    } catch (error) {
                        throw error;
                    }
                },
                createMainData: async (percentage, description, createdById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/CreateTax', {
                            percentage, description, mainCode: state.mainCode, subCode: state.subCode, typeName: state.typeName, createdById
                        });
                        return response;
                    } catch (error) {
                        throw error;
                    }
                },
                updateMainData: async (id, percentage, description, updatedById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/UpdateTax', {
                            id, percentage, description, mainCode: state.mainCode, subCode: state.subCode, typeName: state.typeName, updatedById
                        });
                        return response;
                    } catch (error) {
                        throw error;
                    }
                },
                deleteMainData: async (id, deletedById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/DeleteTax', {
                            id, deletedById
                        });
                        return response;
                    } catch (error) {
                        throw error;
                    }
                },
            };

            const methods = {
                populateMainData: async () => {
                    try {
                    const response = await services.getMainData();
                    const items = response?.data?.content?.data ?? [];

                    console.debug('GetTaxList response items count:', (items || []).length, response);

                const formattedData = (items || []).map(item => ({
                            // keep original fields
                            ...item,
                            // normalize fields so grid can show both tax and tax-register style entries
                            id: item?.id ?? item?.Id ?? item?.ID ?? null,
                            // name removed from grid; keep for backward compatibility but not displayed
                            // name removed from grid; keep for backward compatibility
                            name: item?.name ?? item?.name ?? item?.typeName ?? item?.type ?? null,
                            percentage: (item?.percentage !== undefined && item?.percentage !== null) ? item.percentage : (item?.Percentage ?? null),
                            // display string for grid to avoid template compilation issues
                            percentageDisplay: (item?.percentage !== undefined && item?.percentage !== null)
                                ? (parseFloat(item.percentage ?? item.Percentage).toFixed(2) + ' %')
                                : (item?.Percentage !== undefined && item?.Percentage !== null ? (parseFloat(item.Percentage).toFixed(2) + ' %') : ''),
                            description: item?.description ?? item?.note ?? item?.Description ?? null,
                            mainCode: item?.mainCode ?? item?.MainCode ?? null,
                            subCode: item?.subCode ?? item?.SubCode ?? null,
                            typeName: item?.typeName ?? item?.TypeName ?? null,
                            createdAtUtc: item?.createdAtUtc ? new Date(item.createdAtUtc) : (item?.CreatedAtUtc ? new Date(item.CreatedAtUtc) : null)
                        }));

                        state.mainData = formattedData;
                    } catch (e) {
                        console.warn('Failed to load tax list from API.', e);
                        state.mainData = [];
                    }
                }
            };

            const mainGrid = {
                obj: null,
                create: async (dataSource) => {
                    mainGrid.obj = new ej.grids.Grid({
                        height: '240px',
                        dataSource: dataSource,
                        allowFiltering: true,
                        allowSorting: true,
                        allowSelection: true,
                        allowGrouping: true,
                        allowTextWrap: true,
                        allowResizing: true,
                        allowPaging: true,
                        allowExcelExport: true,
                        filterSettings: { type: 'CheckBox' },
                        sortSettings: { columns: [{ field: 'createdAtUtc', direction: 'Descending' }] },
                        pageSettings: { currentPage: 1, pageSize: 50, pageSizes: ["10", "20", "50", "100", "200", "All"] },
                        selectionSettings: { persistSelection: true, type: 'Single' },
                        autoFit: true,
                        showColumnMenu: true,
                        gridLines: 'Horizontal',
                        columns: [
                            { type: 'checkbox', width: 60 },
                            {
                                field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false
                            },
                            { field: 'mainCode', headerText: 'Main Code', width: 120, textAlign: 'Center' },
                            { field: 'typeName', headerText: 'Type', width: 180 },
                            { field: 'subCode', headerText: 'Sub Code', width: 120, textAlign: 'Center' },
                            { field: 'percentageDisplay', headerText: 'Percentage', width: 100, minWidth: 100 },
                            { field: 'description', headerText: 'Description', width: 400, minWidth: 400 },
                            { field: 'createdAtUtc', headerText: 'Created At UTC', width: 150, format: 'yyyy-MM-dd HH:mm' }
                        ],
                        toolbar: [
                            'ExcelExport', 'Search',
                            { type: 'Separator' },
                            { text: 'Add', tooltipText: 'Add', prefixIcon: 'e-add', id: 'AddCustom' },
                            { text: 'Edit', tooltipText: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' },
                            { text: 'Delete', tooltipText: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' },
                            { type: 'Separator' },
                        ],
                        beforeDataBound: () => { },
                        dataBound: function () {
                            mainGrid.obj.autoFitColumns(['mainCode','typeName','subCode', 'percentage', 'description', 'createdAtUtc']);
                        },
                        excelExportComplete: () => { },
                        rowSelected: () => {
                            if (mainGrid.obj.getSelectedRecords().length === 1) {
                                mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                            } else {
                                mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                            }
                        },
                        rowDeselected: () => {
                            if (mainGrid.obj.getSelectedRecords().length === 1) {
                                mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], true);
                            } else {
                                mainGrid.obj.toolbarModule.enableItems(['EditCustom', 'DeleteCustom'], false);
                            }
                        },
                        rowSelecting: () => {
                            if (mainGrid.obj.getSelectedRecords().length) {
                                mainGrid.obj.clearSelection();
                            }
                        },
                        toolbarClick: (args) => {
                            if (args.item.id === 'MainGrid_excelexport') {
                                mainGrid.obj.excelExport();
                            }

                            if (args.item.id === 'AddCustom') {
                                state.deleteMode = false;
                                state.mainTitle = 'Add Tax';
                                state.id = '';
                                state.mainCode = '';
                                state.subCode = '';
                                state.typeName = '';
                                state.percentage = '';
                                state.description = '';
                                mainModal.obj.show();
                            }

                            if (args.item.id === 'EditCustom') {
                                state.deleteMode = false;
                                if (mainGrid.obj.getSelectedRecords().length) {
                                    const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                    state.mainTitle = 'Edit Tax';
                                    state.id = selectedRecord.id ?? '';
                                    state.mainCode = selectedRecord.mainCode ?? selectedRecord.MainCode ?? '';
                                    state.subCode = selectedRecord.subCode ?? selectedRecord.SubCode ?? '';
                                    state.typeName = selectedRecord.typeName ?? selectedRecord.TypeName ?? '';
                                    state.percentage = selectedRecord.percentage ?? selectedRecord.Percentage ?? '';
                                    state.description = selectedRecord.description ?? '';
                                    mainModal.obj.show();
                                }
                            }

                            if (args.item.id === 'DeleteCustom') {
                                state.deleteMode = true;
                                if (mainGrid.obj.getSelectedRecords().length) {
                                    const selectedRecord = mainGrid.obj.getSelectedRecords()[0];
                                    state.mainTitle = 'Delete Tax?';
                                    state.id = selectedRecord.id ?? '';
                                    state.percentage = selectedRecord.percentage ?? selectedRecord.Percentage ?? '';
                                    state.description = selectedRecord.description ?? '';
                                    state.mainCode = selectedRecord.mainCode ?? selectedRecord.MainCode ?? '';
                                    state.subCode = selectedRecord.subCode ?? selectedRecord.SubCode ?? '';
                                    state.typeName = selectedRecord.typeName ?? selectedRecord.TypeName ?? '';
                                    mainModal.obj.show();
                                }
                            }
                        }
                    });

                    mainGrid.obj.appendTo(mainGridRef.value);
                },
                refresh: () => {
                    mainGrid.obj.setProperties({ dataSource: state.mainData });
                }
            };

            const mainModal = {
                obj: null,
                create: () => {
                    mainModal.obj = new bootstrap.Modal(mainModalRef.value, {
                        backdrop: 'static',
                        keyboard: false
                    });
                }
            };

            // nameText removed (field Name not used)

            const percentageText = {
                obj: null,
                create: () => {
                    percentageText.obj = new ej.inputs.NumericTextBox({
                        placeholder: 'Enter Percentage',
                        format: 'n2',
                        min: 0,
                        max: 100,
                        step: 0.01,
                    });
                    percentageText.obj.appendTo(percentageRef.value);
                },
                refresh: () => {
                    if (percentageText.obj) {
                        percentageText.obj.value = parseFloat(state.percentage);
                    }
                }
            };

            const validateForm = function () {
                state.errors.name = '';
                state.errors.percentage = '';
                state.errors.description = '';

                let isValid = true;

                if (!state.name) {
                    state.errors.name = 'Name is required.';
                    isValid = false;
                }
                if (!state.percentage || isNaN(parseFloat(state.percentage))) {
                    state.errors.percentage = 'Percentage is required and must be a number.';
                    isValid = false;
                } else if (parseFloat(state.percentage) < 0 || parseFloat(state.percentage) > 100) {
                    state.errors.percentage = 'Percentage must be between 0 and 100.';
                    isValid = false;
                }

                return isValid;
            };

            const handler = {
                handleSubmit: async function () {
                    try {
                        state.isSubmitting = true;
                        await new Promise(resolve => setTimeout(resolve, 300));

                        if (!validateForm()) {
                            return;
                        }

                        const response = state.id === ''
                            ? await services.createMainData(state.percentage, state.description, StorageManager.getUserId())
                            : state.deleteMode
                                ? await services.deleteMainData(state.id, StorageManager.getUserId())
                                : await services.updateMainData(state.id, state.percentage, state.description, StorageManager.getUserId());

                        if (response.data.code === 200) {
                            await methods.populateMainData();
                            mainGrid.refresh();
                            Swal.fire({
                                icon: 'success',
                                title: state.deleteMode ? 'Delete Successful' : 'Save Successful',
                                text: 'Form will be closed...',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            setTimeout(() => {
                                mainModal.obj.hide();
                            }, 2000);
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: state.deleteMode ? 'Delete Failed' : 'Save Failed',
                                text: response.data.message ?? 'Please check your data.',
                                confirmButtonText: 'Try Again'
                            });
                        }

                    } catch (error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'An Error Occurred',
                            text: error.response?.data?.message ?? 'Please try again.',
                            confirmButtonText: 'OK'
                        });
                    } finally {
                        state.isSubmitting = false;
                    }
                },
            };

            // name watcher removed

            Vue.watch(
                () => state.percentage,
                (newVal, oldVal) => {
                    state.errors.percentage = '';
                    percentageText.refresh();
                }
            );

        // watchers for register fields (in case custom inputs are used later)
        Vue.watch(() => state.mainCode, () => {});
        Vue.watch(() => state.typeCode, () => {});
        Vue.watch(() => state.subCode, () => {});
        Vue.watch(() => state.typeName, () => {});

            Vue.onMounted(async () => {
                try {
                    await SecurityManager.authorizePage(['Taxs']);
                    await SecurityManager.validateToken();
                    await methods.populateMainData();
                    await mainGrid.create(state.mainData);
                    percentageText.create();
                    mainModal.create();
                } catch (e) {
                    console.error('page init error:', e);
                }
            });

            return {
                mainGridRef,
                mainModalRef,
                percentageRef,
                state,
                handler,
            };
        }
    };

    Vue.createApp(App).mount(mountSelector);
}

// Auto-initialize for existing pages: '#app' or '#tax-register-app'
if (document.querySelector('#app')) {
    createTaxApp('#app');
}

if (document.querySelector('#tax-register-app')) {
    createTaxApp('#tax-register-app');
}
