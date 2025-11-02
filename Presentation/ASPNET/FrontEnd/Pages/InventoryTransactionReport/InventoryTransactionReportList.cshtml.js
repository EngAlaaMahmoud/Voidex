const App = {
    setup() {
        const state = Vue.reactive({
            mainData: []
        });

        const mainGridRef = Vue.ref(null);

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/InventoryTransaction/GetInventoryTransactionReport', {});
                    return response;
                } catch (error) {
                    throw error;
                }
            },
        };

        const methods = {
            populateMainData: async () => {
                const response = await services.getMainData();
                state.mainData = (response?.data?.content?.data || []).map(item => ({
                    ...item,
                    // normalize dates
                    transactionDate: item.transactionDate ? new Date(item.transactionDate) : null,
                    movementDate: item.movementDate ? new Date(item.movementDate) : null,
                    createdAtUtc: item.createdAtUtc ? new Date(item.createdAtUtc) : null
                }));
            }
        };

        Vue.onMounted(async () => {
            try {
                // use TransactionReports role/segment so the page won't redirect
                await SecurityManager.authorizePage(['TransactionReports']);
                await SecurityManager.validateToken();

                await methods.populateMainData();
                await mainGrid.create(state.mainData);

            } catch (e) {
                console.error('page init error:', e);
            }
        });

        const mainGrid = {
            obj: null,
            create: async (dataSource) => {
                mainGrid.obj = new ej.grids.Grid({
                    height: '520px',
                    dataSource: dataSource,
                    enableRtl: true, // right-to-left layout
                    allowFiltering: true,
                    allowSorting: true,
                    allowSelection: true,
                    allowGrouping: false, // keep simple like your screenshot
                    allowTextWrap: true,
                    allowResizing: true,
                    allowPaging: true,
                    allowExcelExport: true,
                    filterSettings: { type: 'CheckBox' },
                    sortSettings: {
                        columns: [
                            { field: 'movementDate', direction: 'Ascending' }
                        ]
                    },
                    pageSettings: { currentPage: 1, pageSize: 200, pageSizes: ["50", "100", "200", "500", "All"] },
                    selectionSettings: { persistSelection: true, type: 'Single' },
                    autoFit: true,
                    showColumnMenu: true,
                    gridLines: 'Both',
                    // Columns arranged R->L visually because enableRtl is true
                    columns: [
                        { type: 'checkbox', width: 60 },
                        { field: 'transactionDate', headerText: 'التاريخ', width: 110, format: 'yyyy-MM-dd', textAlign: 'Center' },
                        { field: 'moduleName', headerText: 'البيان', width: 160, customAttributes: { class: 'module-cell' } },
                        { field: 'purchasePrice', headerText: 'سعر الشراء', width: 110, textAlign: 'Right', format: 'N2' },
                        { field: 'incoming', headerText: 'وارد(+)', width: 90, type: 'number', format: 'N2', textAlign: 'Right', customAttributes: { class: 'incoming-cell' } },
                        { field: 'outgoing', headerText: 'منصرف(-)', width: 90, type: 'number', format: 'N2', textAlign: 'Right', customAttributes: { class: 'outgoing-cell' } },
                        { field: 'purchaseValue', headerText: 'التكلفة', width: 110, type: 'number', format: 'N2', textAlign: 'Right' },
                        { field: 'stock', headerText: 'الرصيد', width: 110, type: 'number', format: 'N2', textAlign: 'Right', customAttributes: { class: 'stock-cell' } },
                        { field: 'productNumber', headerText: 'رقم الصنف', width: 120 },
                        { field: 'productName', headerText: 'اسم الصنف', width: 180, visible: false }, // hidden if not needed visually
                        { field: 'moduleNumber', headerText: 'رقم المستند', width: 140 },
                        { field: 'statusName', headerText: 'الحالة', width: 120, visible: false },
                        { field: 'movementDate', headerText: 'Movement Date', width: 150, format: 'yyyy-MM-dd', visible: false },
                        { field: 'createdAtUtc', headerText: 'Created At', width: 150, format: 'yyyy-MM-dd HH:mm', visible: false }
                    ],
                    aggregates: [
                        {
                            columns: [
                                { type: 'Sum', field: 'incoming', groupFooterTemplate: 'إجمالي الوارد: ${Sum}', groupCaptionTemplate: 'وارد: ${Sum}', format: 'N2' },
                                { type: 'Sum', field: 'outgoing', groupFooterTemplate: 'إجمالي المنصرف: ${Sum}', groupCaptionTemplate: 'منصرف: ${Sum}', format: 'N2' },
                                {
                                    type: 'Custom',
                                    field: 'stock',
                                    groupFooterTemplate: 'الرصيد: ${Custom}',
                                    customAggregate: function (data) {
                                        // return last row stock in the grouped result (running balance)
                                        if (data && data.result && data.result.length > 0) {
                                            const last = data.result[data.result.length - 1];
                                            return last.stock ?? 0;
                                        }
                                        return 0;
                                    },
                                    format: 'N2'
                                },
                                { type: 'Sum', field: 'purchaseValue', groupFooterTemplate: 'إجمالي التكلفة: ${Sum}', format: 'N2' }
                            ]
                        }
                    ],
                    toolbar: ['ExcelExport', 'Search', { type: 'Separator' }],
                    dataBound: function () {
                        // auto fit and apply custom cell styling
                        try {
                            mainGrid.obj.autoFitColumns();

                            const rows = mainGrid.obj.element.querySelectorAll('.e-row');
                            rows.forEach(row => {
                                // stock cell styling (negative = red, zero = yellow)
                                const stockCell = row.querySelector('.stock-cell');
                                if (stockCell) {
                                    const stockValue = parseFloat(stockCell.textContent.replace(/,/g, ''));
                                    if (!isNaN(stockValue)) {
                                        if (stockValue < 0) {
                                            stockCell.style.backgroundColor = '#ffcccc';
                                            stockCell.style.fontWeight = 'bold';
                                        } else if (stockValue === 0) {
                                            stockCell.style.backgroundColor = '#ffffcc';
                                        } else {
                                            // positive - subtle background removed
                                        }
                                    }
                                }

                                // incoming/outgoing coloring
                                const incomingCell = row.querySelector('.incoming-cell');
                                if (incomingCell && incomingCell.textContent.trim()) {
                                    incomingCell.style.backgroundColor = '#ccffcc';
                                }
                                const outgoingCell = row.querySelector('.outgoing-cell');
                                if (outgoingCell && outgoingCell.textContent.trim()) {
                                    outgoingCell.style.backgroundColor = '#ffcccc';
                                }

                                // module cell colouring based on content (example keywords)
                                const moduleCell = row.querySelector('.module-cell');
                                if (moduleCell) {
                                    const txt = moduleCell.textContent.trim();
                                    if (txt.includes('بيع')) {
                                        moduleCell.style.backgroundColor = '#ffff66'; // yellow-ish for sales
                                    } else if (txt.includes('شراء')) {
                                        moduleCell.style.backgroundColor = '#cce5ff'; // blue-ish for purchase
                                    } else if (txt.includes('تحويل')) {
                                        moduleCell.style.backgroundColor = '#e6f7e6'; // light green for transfers
                                    }
                                }
                            });
                        } catch (err) {
                            console.warn('dataBound styling error', err);
                        }
                    },
                    toolbarClick: (args) => {
                        if (args.item && args.item.id && args.item.id.toLowerCase().includes('excelexport')) {
                            mainGrid.obj.excelExport({
                                fileName: 'InventoryTransactionReport_' + new Date().toISOString().split('T')[0] + '.xlsx'
                            });
                        }
                    }
                });

                mainGrid.obj.appendTo(mainGridRef.value);
            },
            refresh: () => {
                if (mainGrid.obj) {
                    mainGrid.obj.setProperties({ dataSource: state.mainData });
                }
            }
        };

        return {
            mainGridRef,
            state,
        };
    }
};

Vue.createApp(App).mount('#app');