import i18n from 'i18next';
import * as resources from '../../assets/locales';

i18n.init({
    resources: resources,
    fallbackLng: "en"
});

export default i18n;