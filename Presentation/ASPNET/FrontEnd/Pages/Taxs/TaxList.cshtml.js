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
                taxCategoryId: '',          // ← new: category ID
                taxType: '',                // keep for display/legacy
                percentage: '',
                description: '',
                taxCategories: [],          // ← list of categories from API
                errors: {
                    percentage: '',
                    taxCategoryId: ''
                },
                isSubmitting: false,
            });

            const mainGridRef = Vue.ref(null);
            const mainModalRef = Vue.ref(null);
            const percentageRef = Vue.ref(null);
            const taxCategoryRef = Vue.ref(null);  // ← new ref for dropdown
            const services = {
                getMainData: async () => {
                    try {
                        const response = await AxiosManager.get('/Tax/GetTaxList', {});
                        return response;
                    } catch (error) { throw error; }
                },
                getTaxCategories: async () => {
                    try {
                        const res = await AxiosManager.get('/Tax/GetTaxCategories', {});
                        console.log('Loaded categories:', res?.data?.content?.data);
                        return res?.data?.content?.data ?? [];
                    } catch (err) {
                        console.error('Failed to load categories:', err);
                        Swal.fire({ icon: 'warning', title: 'تحذير', text: 'تعذر تحميل فئات الضرائب' });
                        return [];
                    }
                },
                createMainData: async (payload, createdById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/CreateTax', {
                            ...payload,
                            createdById
                        });
                        return response;
                    } catch (error) { throw error; }
                },
                updateMainData: async (payload, updatedById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/UpdateTax', {
                            ...payload,
                            updatedById
                        });
                        return response;
                    } catch (error) { throw error; }
                },
                deleteMainData: async (id, deletedById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/DeleteTax', { id, deletedById });
                        return response;
                    } catch (error) { throw error; }
                },
            };
            

            const methods = {
                populateMainData: async () => {
                    try {
                        const response = await services.getMainData();
                        const items = response?.data?.content?.data ?? [];

                        const formattedData = (items || []).map(item => ({
                            ...item,
                            id: item?.id ?? item?.Id ?? item?.ID ?? null,
                            percentage: (item?.percentage !== undefined && item?.percentage !== null) ? item.percentage : (item?.Percentage ?? null),
                            percentageDisplay: (item?.percentage !== undefined && item?.percentage !== null)
                                ? (parseFloat(item.percentage).toFixed(2) + ' %')
                                : (item?.Percentage !== undefined && item?.Percentage !== null ? (parseFloat(item.Percentage).toFixed(2) + ' %') : ''),
                            description: item?.description ?? item?.note ?? item?.Description ?? null,
                            mainCode: item?.mainCode ?? item?.MainCode ?? null,
                            subCode: item?.subCode ?? item?.SubCode ?? null,
                            taxCategoryName: item?.taxCategoryName ?? '',  // ← assume backend sends it
                            taxType: item?.taxType ?? item?.typeName ?? item?.TypeName ?? null, // جلب البيانات من المسميين القديم والجديد
                            createdAtUtc: item?.createdAtUtc ? new Date(item.createdAtUtc) : (item?.CreatedAtUtc ? new Date(item.CreatedAtUtc) : null)
                        }));

                        state.mainData = formattedData;
                    } catch (e) {
                        console.warn('Failed to load tax list.', e);
                        state.mainData = [];
                    }
                },
                populateTaxCategories: async () => {
                    state.taxCategories = await services.getTaxCategories();
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
                        allowPaging: true,
                        filterSettings: { type: 'CheckBox' },
                        sortSettings: { columns: [{ field: 'createdAtUtc', direction: 'Descending' }] },
                        pageSettings: { pageSize: 50 },
                        selectionSettings: { type: 'Single' },
                        columns: [
                            { field: 'mainCode', headerText: 'Main Code', width: '100' },
                            { field: 'subCode', headerText: 'Sub Code', width: '120' },
                            { field: 'taxCategoryName', headerText: 'Category', width: '180' },  // ← new
                            { field: 'taxType', headerText: 'Tax Type (legacy)', width: '180' },
                            { field: 'description', headerText: 'Description', width: '250' },
                            { field: 'percentageDisplay', headerText: 'Percentage', textAlign: 'right', width: '110' },
                            { field: 'createdAtUtc', headerText: 'Created', format: 'yMd', width: '140' }
                        ],
                        toolbar: ['ExcelExport', 'Search', { text: 'Add', prefixIcon: 'e-add', id: 'AddCustom' }, { text: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' }, { text: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' }],
                        dataBound: function () {
                            mainGrid.obj.autoFitColumns(['mainCode', 'taxType', 'subCode', 'percentageDisplay', 'description', 'createdAtUtc']);
                        },
                        toolbarClick: (args) => {
                            if (args.item.id === 'AddCustom') {
                                methods.addNew();
                            }
                            if (args.item.id === 'EditCustom' && mainGrid.obj.getSelectedRecords().length) {
                                const selected = mainGrid.obj.getSelectedRecords()[0];
                                methods.editRecord(selected);
                            }
                            if (args.item.id === 'DeleteCustom' && mainGrid.obj.getSelectedRecords().length) {
                                const selected = mainGrid.obj.getSelectedRecords()[0];
                                state.deleteMode = true;
                                state.mainTitle = 'Delete Tax?';
                                state.id = selected.id;
                                state.taxType = selected.taxType;
                                mainModal.obj.show();
                            }
                        }
                    });
                    mainGrid.obj.appendTo(mainGridRef.value);
                },
                refresh: () => { mainGrid.obj.setProperties({ dataSource: state.mainData }); }
            };

            const mainModal = {
                obj: null,
                create: () => {
                    mainModal.obj = new bootstrap.Modal(mainModalRef.value, { backdrop: 'static' });
                }
            };
            const taxCategoryDropdown = {
                obj: null,
                createOrRefresh: () => {
                    console.log('Creating/refreshing dropdown with value:', state.taxCategoryId);

                    // Destroy old instance if exists
                    if (taxCategoryDropdown.obj) {
                        try {
                            taxCategoryDropdown.obj.destroy();
                            taxCategoryDropdown.obj = null;
                            console.log('Destroyed old dropdown instance');
                        } catch (e) {
                            console.warn('Error destroying dropdown:', e);
                        }
                    }

                    // Get the container - use DOM selector instead of ref
                    const container = document.getElementById('taxCategoryContainer');
                    if (!container) {
                        console.error('taxCategoryContainer not found in DOM');
                        return;
                    }

                    // Clear container
                    container.innerHTML = '';

                    // Create a fresh input element for the dropdown
                    const inputId = 'taxCategoryDropdown_' + Date.now();
                    const inputHtml = `<input id="${inputId}" type="text" />`;
                    container.innerHTML = inputHtml;

                    const inputElement = document.getElementById(inputId);

                    if (!inputElement) {
                        console.error('Failed to create input element');
                        return;
                    }

                    // Create dropdown
                    taxCategoryDropdown.obj = new ej.dropdowns.DropDownList({
                        dataSource: state.taxCategories || [],
                        fields: { value: 'id', text: 'nameAr' },
                        placeholder: 'اختر فئة الضريبة',
                        popupHeight: '250px',
                        floatLabelType: 'Auto',
                        enabled: true,
                        allowFiltering: true,
                        filtering: function (e) {
                            let query = new ej.data.Query();
                            query = (e.text !== '') ? query.where('nameAr', 'contains', e.text, true) : query;
                            e.updateData(state.taxCategories, query);
                        },
                        change: (e) => {
                            state.taxCategoryId = e.value;
                            state.errors.taxCategoryId = '';
                            console.log('Category selected:', e.value);
                        },
                        created: function () {
                            console.log('Dropdown created successfully');
                        }
                    });

                    // Append to the input element
                    taxCategoryDropdown.obj.appendTo(inputElement);

                    // Set the value if it exists
                    if (state.taxCategoryId) {
                        console.log('Setting dropdown value to:', state.taxCategoryId);
                        taxCategoryDropdown.obj.value = state.taxCategoryId;
                    }

                    // Refresh and data bind
                    setTimeout(() => {
                        if (taxCategoryDropdown.obj) {
                            taxCategoryDropdown.obj.dataBind();
                            taxCategoryDropdown.obj.refresh();
                            console.log('Dropdown initialized with value:', taxCategoryDropdown.obj.value);
                        }
                    }, 50);
                },
                destroy: () => {
                    if (taxCategoryDropdown.obj) {
                        try {
                            taxCategoryDropdown.obj.destroy();
                            taxCategoryDropdown.obj = null;
                        } catch (e) {
                            console.warn('Error destroying dropdown:', e);
                        }
                    }
                    // Clear container
                    const container = document.getElementById('taxCategoryContainer');
                    if (container) {
                        container.innerHTML = '';
                    }
                }
            };
            const percentageText = {
                obj: null,
                create: () => {
                    percentageText.obj = new ej.inputs.NumericTextBox({ format: 'n2', min: 0, max: 100 });
                    percentageText.obj.appendTo(percentageRef.value);
                },
                refresh: () => { if (percentageText.obj) percentageText.obj.value = parseFloat(state.percentage); }
            };

            const validateForm = () => {
                state.errors = { percentage: '', taxCategoryId: '' };
                let valid = true;

                if (!state.percentage || isNaN(parseFloat(state.percentage))) {
                    state.errors.percentage = 'Percentage is required.';
                    valid = false;
                }
                if (!state.taxCategoryId) {
                    state.errors.taxCategoryId = 'Tax Category is required.';
                    valid = false;
                }
                return valid;
            };

            const handler = {
                handleSubmit: async () => {
                    if (!state.deleteMode && !validateForm()) return;

                    state.isSubmitting = true;
                    try {
                        const payload = {
                            id: state.id,
                            mainCode: state.mainCode,
                            subCode: state.subCode,
                            taxCategoryId: state.taxCategoryId,     // ← send ID
                            percentage: parseFloat(state.percentage),
                            description: state.description
                        };

                        let response;
                        if (state.id === '') {
                            response = await services.createMainData(payload, StorageManager.getUserId());
                        } else if (state.deleteMode) {
                            response = await services.deleteMainData(state.id, StorageManager.getUserId());
                        } else {
                            response = await services.updateMainData(payload, StorageManager.getUserId());
                        }

                        if (response.data.code === 200) {
                            await methods.populateMainData();
                            mainGrid.refresh();
                            Swal.fire({ icon: 'success', title: 'Success', timer: 2000, showConfirmButton: false });
                            mainModal.obj.hide();
                        }
                    } catch (error) {
                        Swal.fire({ icon: 'error', title: 'Error', text: error.message || 'Operation failed' });
                    } finally {
                        state.isSubmitting = false;
                    }
                }
            };

            Vue.onMounted(async () => {
                try {
                    await SecurityManager.authorizePage(['Taxs']);
                    await methods.populateTaxCategories();   // load categories first
                    await methods.populateMainData();
                    await mainGrid.create(state.mainData);
                    percentageText.create();
                    mainModal.create();
                    mainModalRef.value.addEventListener('hidden.bs.modal', () => {
                        state.id = '';
                        state.mainCode = '';
                        state.subCode = '';
                        state.taxCategoryId = '';
                        state.taxType = '';
                        state.percentage = '';
                        state.description = '';
                        state.deleteMode = false;
                        state.errors = {};
                        percentageText.refresh();
                        //taxCategoryDropdown.destroy();
                        // Reset the form state
                        if (percentageText.obj) {
                            percentageText.obj.value = null;
                            percentageText.obj.refresh();
                        }

                        // Clean up dropdown
                        taxCategoryDropdown.destroy();

                    });
                    //mainModalRef.value.addEventListener('shown.bs.modal', () => {
                    //        // Wait for Vue to update the DOM
                    //        Vue.nextTick(() => {
                    //            console.log('Modal shown, creating dropdown for state.taxCategoryId:', state.taxCategoryId);
                    //            console.log('Available categories:', state.taxCategories);

                    //            // Destroy any existing dropdown first
                    //            taxCategoryDropdown.destroy();

                    //            // Create fresh dropdown
                    //            if (taxCategoryRef.value) {
                    //                taxCategoryDropdown.createOrRefresh();

                    //                // IMPORTANT: Set the value AFTER creating the dropdown
                    //                if (state.taxCategoryId) {
                    //                    taxCategoryDropdown.obj.value = state.taxCategoryId;
                    //                    taxCategoryDropdown.obj.dataBind();
                    //                }
                    //            }
                    //        });
                    //    });
                    mainModalRef.value.addEventListener('shown.bs.modal', () => {
                        Vue.nextTick(() => {
                            taxCategoryDropdown.createOrRefresh();
                        });
                    });
                }
                 catch (e) { console.error(e); }

                // ── Add this new function to open modal (replace your existing open logic)
            });


            methods.addNew = () => {
                state.deleteMode = false;
                state.mainTitle = 'Add Tax';
                state.id = '';
                state.mainCode = '';
                state.subCode = '';
                state.taxCategoryId = '';  // Explicitly set to empty
                state.taxType = '';
                state.percentage = '';
                state.description = '';
                state.errors = {};

                // Reset percentage control
                if (percentageText.obj) {
                    percentageText.obj.value = null;
                    percentageText.obj.refresh();
                }

                // Show modal - dropdown will be created in shown.bs.modal event
                mainModal.obj.show();
            };

            methods.editRecord = (record) => {
                state.deleteMode = false;
                state.mainTitle = 'Edit Tax';
                state.id = record.id;
                state.mainCode = record.mainCode || '';
                state.subCode = record.subCode || '';
                state.taxCategoryId = record.taxCategoryId || '';
                state.taxType = record.taxType || '';
                state.percentage = record.percentage ?? '';
                state.description = record.description || '';
                state.errors = {};

                // Set percentage control
                if (percentageText.obj && state.percentage) {
                    percentageText.obj.value = parseFloat(state.percentage);
                    percentageText.obj.refresh();
                }

                // Show modal - dropdown will be created in shown.bs.modal event
                mainModal.obj.show();
            };

            return { mainGridRef, mainModalRef, percentageRef,state, handler };
        }
    };
    Vue.createApp(App).mount(mountSelector);
}

if (document.querySelector('#app')) { createTaxApp('#app'); }