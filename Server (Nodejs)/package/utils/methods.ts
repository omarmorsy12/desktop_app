import { JsonRef } from "../models/utils-model";

export function isNullOrUndefined(object: any) {
    return object === undefined || object === null;
}

export function isBoolean(object: any) {
    return typeof object === 'boolean';
};

export function getJsonRefs(obj: any, exclude?: any[], currentPath = '', refs: JsonRef[] = []): JsonRef[] {
  const keys = Object.keys(obj);
  
  keys.forEach(key => {
      const path = currentPath ? (currentPath + '.' + key) : key;

      const isObject = typeof(obj[key]) === 'object';
      const notNull = !isNullOrUndefined(obj[key]);
      const notArray = !Array.isArray(obj[key]);
      const notExcluded = !exclude || !exclude.find(t => obj[key] instanceof t);

      if (notNull && isObject && notArray && notExcluded) {
        getJsonRefs(obj[key], exclude, path, refs);
      } else {
        refs.push({ key, path, value: obj[key], obj });
      }
  });

  return refs;
}
