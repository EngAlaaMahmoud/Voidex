const TaxRegisterApp = {
    setup() {
        const state = Vue.reactive({
            mainData: [],
        });

        const registerGridRef = Vue.ref(null);

        // Static dataset as fallback
        const taxRows = [
            { mainCode: 'T_1', type: 'VAT', subCode: 'V001', description: 'ÊÕÏíÑ ááÎÇÑÌ' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V002', description: 'ÊÕÏíÑ áãäÇØÞ ÍÑÉ æÃÎÑì' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V003', description: 'ÓáÚÉ Ãæ ÎÏãÉ ãÚÝÇÉ' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V004', description: 'ÓáÚÉ Ãæ ÎÏãÉ ÛíÑ ÎÇÖÚÉ' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V005', description: 'áÈíÇä' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V006', description: 'ÅÚÝÇÁÇÊ ÏÝÇÚ æÃãä Þæãí' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V007', description: 'ÅÚÝÇÁÇÊ ÇÊÝÇÞíÇÊ' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V008', description: 'ÅÚÝÇÁÇÊ ÃÎÑì' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V009', description: 'ÓáÚ ÚÇãÉ' },
            { mainCode: 'T_1', type: 'VAT', subCode: 'V010', description: 'äÓÈ ÃÎÑì' },
            { mainCode: 'T_2', type: 'ÖÑíÈå ÌÏæá', subCode: 'Tbl01', description: 'ÖÑíÈÉ ÇáÌÏæá – äÓÈíÉ' },
            { mainCode: 'T_3', type: 'ÖÑíÈå ÌÏæá', subCode: 'Tbl02', description: 'ÖÑíÈÉ ÇáÌÏæá – äæÚíÉ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W001', description: 'ÇáãÞÇæáÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W002', description: 'ÇáÊæÑíÏÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W003', description: 'ÇáãÔÊÑíÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W004', description: 'ÇáÎÏãÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W005', description: 'ÇáÌãÚíÇÊ ÇáÊÚÇæäíÉ ááäÞá' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W006', description: 'ÇáæßÇáÉ æÇáÓãÓÑÉ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W007', description: 'ÔÑßÇÊ ÇáÏÎÇä æÇáÇÓãäÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W008', description: 'ÔÑßÇÊ ÇáÈÊÑæá æÇáÇÊÕÇáÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W009', description: 'ÏÚã ÇáÕÇÏÑÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W010', description: 'ÃÊÚÇÈ ãåäíÉ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W011', description: 'ÚãæáÉ æÓãÓÑÉ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W012', description: 'ÊÍÕíá ÇáãÓÊÔÝíÇÊ ãä ÇáÃØÈÇÁ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W013', description: 'ÅÊÇæÇÊ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W014', description: 'ÊÎáíÕ ÌãÑßí' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W015', description: 'ÅÚÝÇÁ' },
            { mainCode: 'T_4', type: 'ÇáÎÕã ÊÍÊ ÍÓÇÈ ÖÑíÈå', subCode: 'W016', description: 'ÏÝÚÇÊ ãÞÏãÉ' },
            { mainCode: 'T_5', type: 'ÖÑíÈå ÇáÏãÛå', subCode: 'ST01', description: 'ÖÑíÈÉ ÇáÏãÛÉ – äÓÈíÉ' },
            { mainCode: 'T_6', type: 'ÖÑíÈå ÇáÏãÛå', subCode: 'ST02', description: 'ÖÑíÈÉ ÇáÏãÛÉ – ÞØÚíÉ' },
            { mainCode: 'T_7', type: 'ÖÑíÈå ÇáãáÇåì', subCode: 'Ent01', description: 'ÖÑíÈÉ ÇáãáÇåí äÓÈíÉ' },
            { mainCode: 'T_7', type: 'ÖÑíÈå ÇáãáÇåì', subCode: 'Ent02', description: 'ÖÑíÈÉ ÇáãáÇåí ÞØÚíÉ' },
            { mainCode: 'T_8', type: 'ÑÓã Êäãíå ÇáãæÇÑÏ', subCode: 'RD01', description: 'ÑÓã ÊäãíÉ ÇáãæÇÑÏ – äÓÈíÉ' },
            { mainCode: 'T_8', type: 'ÑÓã Êäãíå ÇáãæÇÑÏ', subCode: 'RD02', description: 'ÑÓã ÊäãíÉ ÇáãæÇÑÏ – ÞØÚíÉ' },
            { mainCode: 'T_9', type: 'ÑÓã ÎÏãå', subCode: 'RD01', description: 'ÑÓã ÎÏãÉ – äÓÈíÉ' },
            { mainCode: 'T_9', type: 'ÑÓã ÎÏãå', subCode: 'RD02', description: 'ÑÓã ÎÏãÉ – ÞØÚíÉ' },
            { mainCode: 'T_10', type: 'ÑÓã ãÍáíÇÊ', subCode: 'Mn01', description: 'ÑÓã ÇáãÍáíÇÊ – äÓÈÉ' },
            { mainCode: 'T_10', type: 'ÑÓã ãÍáíÇÊ', subCode: 'Mn02', description: 'ÑÓã ÇáãÍáíÇÊ – ÞØÚíÉ' },
            { mainCode: 'T_11', type: 'ÑÓã ÇáÊÇãíä ÇáÕÍì', subCode: 'MI01', description: 'ÑÓã ÇáÊÃãíä ÇáÕÍí – äÓÈÉ' },
            { mainCode: 'T_11', type: 'ÑÓã ÇáÊÇãíä ÇáÕÍì', subCode: 'MI02', description: 'ÑÓã ÇáÊÃãíä ÇáÕÍí – ÞØÚíÉ' },
            { mainCode: 'T_12', type: 'ÑÓæã ÇÎÑì', subCode: 'OF01', description: 'ÑÓæã ÃÎÑì – äÓÈÉ' },
            { mainCode: 'T_12', type: 'ÑÓæã ÇÎÑì', subCode: 'OF02', description: 'ÑÓæã ÃÎÑì – ÞØÚíÉ' },
            { mainCode: 'T_13', type: 'ÖÑíÈå ÏãÛå', subCode: 'ST03', description: 'ÖÑíÈÉ ÇáÏãÛÉ äÓÈíÉ (ÛíÑ ÖÑíÈí)' },
            { mainCode: 'T_14', type: 'ÖÑíÈå ÏãÛå', subCode: 'ST04', description: 'ÖÑíÈÉ ÇáÏãÛÉ ÞØÚíÉ (ÛíÑ ÖÑíÈí)' },
            { mainCode: 'T_15', type: 'ÖÑíÈå ãáÇåì', subCode: 'Ent03', description: 'ÖÑíÈÉ ÇáãáÇåí äÓÈíÉ' },
            { mainCode: 'T_15', type: 'ÖÑíÈå ãáÇåì', subCode: 'Ent04', description: 'ÖÑíÈÉ ÇáãáÇåí ÞØÚíÉ' },
            { mainCode: 'T_16', type: 'ÑÓæã Êäãíå ÇáãæÇÑÏ', subCode: 'RD03', description: 'ÑÓã ÊäãíÉ ÇáãæÇÑÏ – äÓÈÉ' },
            { mainCode: 'T_16', type: 'ÑÓæã Êäãíå ÇáãæÇÑÏ', subCode: 'RD04', description: 'ÑÓã ÊäãíÉ ÇáãæÇÑÏ – ÞØÚíÉ' },
            { mainCode: 'T_17', type: 'ÑÓæã ÎÏãå', subCode: 'SC03', description: 'ÑÓã ÎÏãÉ – äÓÈÉ' },
            { mainCode: 'T_17', type: 'ÑÓæã ÎÏãå', subCode: 'SC04', description: 'ÑÓã ÎÏãÉ – ÞØÚíÉ' },
            { mainCode: 'T_18', type: 'ÑÓã ÇáãÍáíÇÊ', subCode: 'Mn03', description: 'ÑÓã ÇáãÍáíÇÊ – äÓÈÉ' },
            { mainCode: 'T_18', type: 'ÑÓã ÇáãÍáíÇÊ', subCode: 'Mn04', description: 'ÑÓã ÇáãÍáíÇÊ – ÞØÚíÉ' },
            { mainCode: 'T_19', type: 'ÑÓæã ÇáÊÇãíä ÇáÕÍì', subCode: 'MI03', description: 'ÑÓã ÇáÊÃãíä ÇáÕÍí – äÓÈÉ' },
            { mainCode: 'T_19', type: 'ÑÓæã ÇáÊÇãíä ÇáÕÍì', subCode: 'MI04', description: 'ÑÓã ÇáÊÃãíä ÇáÕÍí – ÞØÚíÉ' },
            { mainCode: 'T_20', type: 'ÑÓæã ÃÎÑì', subCode: 'OF03', description: 'ÑÓæã ÃÎÑì – äÓÈÉ' },
            { mainCode: 'T_20', type: 'ÑÓæã ÃÎÑì', subCode: 'OF04', description: 'ÑÓæã ÃÎÑì – ÞØÚíÉ' },
        ];

        const services = {
            getMainData: async () => {
                try {
                    const response = await AxiosManager.get('/Tax/GetTaxList', {});
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
                    const formattedData = response?.data?.content?.data?.map(item => ({
                        mainCode: item.mainCode ?? item.typeCode ?? item.typeId ?? 'T_1',
                        type: item.typeName ?? item.name ?? item.type ?? 'VAT',
                        subCode: item.code ?? item.subCode ?? item.shortCode ?? '',
                        description: item.description ?? item.note ?? ''
                    })) || [];

                    // If API returns empty, fallback to static list
                    state.mainData = formattedData.length ? formattedData : taxRows;
                } catch (e) {
                    console.warn('Failed to load tax list from API, using fallback static data.', e);
                    state.mainData = taxRows;
                }
            }
        };

        const registerGrid = {
            obj: null,
            create: async (dataSource) => {
                registerGrid.obj = new ej.grids.Grid({
                    height: '600px',
                    dataSource: dataSource,
                    allowPaging: true,
                    pageSettings: { pageSize: 20, pageSizes: ['10','20','50','100'] },
                    allowSorting: true,
                    allowFiltering: true,
                    filterSettings: { type: 'Menu' },
                    allowResizing: true,
                    allowTextWrap: true,
                    toolbar: ['Search', 'ExcelExport', 'PdfExport', 'CsvExport', 'Print'],
                    columns: [
                        { field: 'mainCode', headerText: 'ÇáßæÏ ÇáÑÆíÓí', width: 120, textAlign: 'Center' },
                        { field: 'type', headerText: 'äæÚ ÇáÖÑíÈå', width: 220 },
                        { field: 'subCode', headerText: 'ÇáÝÑÚì', width: 120, textAlign: 'Center' },
                        { field: 'description', headerText: 'ÇáÈíÇä', width: 500 },
                    ],
                    locale: 'ar-AE',
                    enableRtl: true,
                    dataBound: function () {
                        // any post data-bound adjustments
                    },
                    toolbarClick: function (args) {
                        const id = args.item.id || '';
                        const lower = id.toLowerCase();
                        if (lower.indexOf('excelexport') !== -1) {
                            registerGrid.obj.excelExport();
                        } else if (lower.indexOf('pdfexport') !== -1) {
                            registerGrid.obj.pdfExport();
                        } else if (lower.indexOf('csvexport') !== -1) {
                            registerGrid.obj.csvExport();
                        } else if (lower.indexOf('print') !== -1) {
                            registerGrid.obj.print();
                        }
                    }
                });

                registerGrid.obj.appendTo(registerGridRef.value);
            },
            refresh: () => {
                registerGrid.obj.setProperties({ dataSource: state.mainData });
            }
        };

        Vue.onMounted(async () => {
            try {
                await methods.populateMainData();
                await registerGrid.create(state.mainData);
            } catch (e) {
                console.error('page init error:', e);
            }
        });

        return {
            registerGridRef,
            state,
        };
    }
};

Vue.createApp(TaxRegisterApp).mount('#tax-register-app');
