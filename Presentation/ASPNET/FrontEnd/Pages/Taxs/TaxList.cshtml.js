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
                taxType: '', // تم التعديل هنا
                percentage: '',
                description: '',
                errors: {
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
                    } catch (error) { throw error; }
                },
                createMainData: async (percentage, description, createdById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/CreateTax', {
                            percentage, description, mainCode: state.mainCode, subCode: state.subCode, typeName: state.taxType, createdById
                        });
                        return response;
                    } catch (error) { throw error; }
                },
                updateMainData: async (id, percentage, description, updatedById) => {
                    try {
                        const response = await AxiosManager.post('/Tax/UpdateTax', {
                            id, percentage, description, mainCode: state.mainCode, subCode: state.subCode, typeName: state.taxType, updatedById
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
                            taxType: item?.taxType ?? item?.typeName ?? item?.TypeName ?? null, // جلب البيانات من المسميين القديم والجديد
                            createdAtUtc: item?.createdAtUtc ? new Date(item.createdAtUtc) : (item?.CreatedAtUtc ? new Date(item.CreatedAtUtc) : null)
                        }));

                        state.mainData = formattedData;
                    } catch (e) {
                        console.warn('Failed to load tax list.', e);
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
                        allowPaging: true,
                        filterSettings: { type: 'CheckBox' },
                        sortSettings: { columns: [{ field: 'createdAtUtc', direction: 'Descending' }] },
                        pageSettings: { pageSize: 50 },
                        selectionSettings: { type: 'Single' },
                        columns: [
                            { type: 'checkbox', width: 60 },
                            { field: 'id', isPrimaryKey: true, headerText: 'Id', visible: false },
                            { field: 'mainCode', headerText: 'Main Code', width: 120, textAlign: 'Center' },
                            { field: 'taxType', headerText: 'Tax Type', width: 180 }, // تم تعديل Header النص هنا
                            { field: 'subCode', headerText: 'Sub Code', width: 120, textAlign: 'Center' },
                            { field: 'percentageDisplay', headerText: 'Percentage', width: 100 },
                            { field: 'description', headerText: 'Description', width: 400 },
                            { field: 'createdAtUtc', headerText: 'Created At', width: 150, format: 'yyyy-MM-dd HH:mm' }
                        ],
                        toolbar: ['ExcelExport', 'Search', { text: 'Add', prefixIcon: 'e-add', id: 'AddCustom' }, { text: 'Edit', prefixIcon: 'e-edit', id: 'EditCustom' }, { text: 'Delete', prefixIcon: 'e-delete', id: 'DeleteCustom' }],
                        dataBound: function () {
                            mainGrid.obj.autoFitColumns(['mainCode', 'taxType', 'subCode', 'percentageDisplay', 'description', 'createdAtUtc']);
                        },
                        toolbarClick: (args) => {
                            if (args.item.id === 'AddCustom') {
                                state.deleteMode = false;
                                state.mainTitle = 'Add Tax';
                                state.id = ''; state.mainCode = ''; state.subCode = ''; state.taxType = ''; state.percentage = ''; state.description = '';
                                mainModal.obj.show();
                            }
                            if (args.item.id === 'EditCustom' && mainGrid.obj.getSelectedRecords().length) {
                                const selected = mainGrid.obj.getSelectedRecords()[0];
                                state.deleteMode = false;
                                state.mainTitle = 'Edit Tax';
                                state.id = selected.id;
                                state.mainCode = selected.mainCode;
                                state.subCode = selected.subCode;
                                state.taxType = selected.taxType; // ربط الحقل المعدل
                                state.percentage = selected.percentage;
                                state.description = selected.description;
                                mainModal.obj.show();
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

            const percentageText = {
                obj: null,
                create: () => {
                    percentageText.obj = new ej.inputs.NumericTextBox({ format: 'n2', min: 0, max: 100 });
                    percentageText.obj.appendTo(percentageRef.value);
                },
                refresh: () => { if (percentageText.obj) percentageText.obj.value = parseFloat(state.percentage); }
            };

            const validateForm = function () {
                state.errors.percentage = '';
                if (!state.percentage || isNaN(parseFloat(state.percentage))) {
                    state.errors.percentage = 'Percentage is required.';
                    return false;
                }
                return true;
            };

            const handler = {
                handleSubmit: async function () {
                    try {
                        if (!state.deleteMode && !validateForm()) return;
                        state.isSubmitting = true;
                        const response = state.id === ''
                            ? await services.createMainData(state.percentage, state.description, StorageManager.getUserId())
                            : state.deleteMode
                                ? await services.deleteMainData(state.id, StorageManager.getUserId())
                                : await services.updateMainData(state.id, state.percentage, state.description, StorageManager.getUserId());

                        if (response.data.code === 200) {
                            await methods.populateMainData();
                            mainGrid.refresh();
                            Swal.fire({ icon: 'success', title: 'Success', timer: 2000, showConfirmButton: false });
                            mainModal.obj.hide();
                        }
                    } catch (error) {
                        Swal.fire({ icon: 'error', title: 'Error' });
                    } finally { state.isSubmitting = false; }
                },
            };

            Vue.onMounted(async () => {
                try {
                    await SecurityManager.authorizePage(['Taxs']);
                    await methods.populateMainData();
                    await mainGrid.create(state.mainData);
                    percentageText.create();
                    mainModal.create();
                } catch (e) { console.error(e); }
            });

            return { mainGridRef, mainModalRef, percentageRef, state, handler };
        }
    };
    Vue.createApp(App).mount(mountSelector);
}

if (document.querySelector('#app')) { createTaxApp('#app'); }