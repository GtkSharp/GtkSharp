
#ifndef __g_cclosure_user_marshal_MARSHAL_H__
#define __g_cclosure_user_marshal_MARSHAL_H__

#include	<glib-object.h>

G_BEGIN_DECLS

/* VOID:OBJECT,DOUBLE,POINTER,INT (canvas-proxy-marshal.list:1) */
extern void g_cclosure_user_marshal_VOID__OBJECT_DOUBLE_POINTER_INT (GClosure     *closure,
                                                                     GValue       *return_value,
                                                                     guint         n_param_values,
                                                                     const GValue *param_values,
                                                                     gpointer      invocation_hint,
                                                                     gpointer      marshal_data);

/* DOUBLE:OBJECT,DOUBLE,DOUBLE,INT,INT,POINTER (canvas-proxy-marshal.list:2) */
extern void g_cclosure_user_marshal_DOUBLE__OBJECT_DOUBLE_DOUBLE_INT_INT_POINTER (GClosure     *closure,
                                                                                  GValue       *return_value,
                                                                                  guint         n_param_values,
                                                                                  const GValue *param_values,
                                                                                  gpointer      invocation_hint,
                                                                                  gpointer      marshal_data);

/* VOID:OBJECT,POINTER,POINTER,POINTER,POINTER (canvas-proxy-marshal.list:3) */
extern void g_cclosure_user_marshal_VOID__OBJECT_POINTER_POINTER_POINTER_POINTER (GClosure     *closure,
                                                                                  GValue       *return_value,
                                                                                  guint         n_param_values,
                                                                                  const GValue *param_values,
                                                                                  gpointer      invocation_hint,
                                                                                  gpointer      marshal_data);

/* VOID:OBJECT,INT,INT,INT,INT (canvas-proxy-marshal.list:4) */
extern void g_cclosure_user_marshal_VOID__OBJECT_INT_INT_INT_INT (GClosure     *closure,
                                                                  GValue       *return_value,
                                                                  guint         n_param_values,
                                                                  const GValue *param_values,
                                                                  gpointer      invocation_hint,
                                                                  gpointer      marshal_data);

G_END_DECLS

#endif /* __g_cclosure_user_marshal_MARSHAL_H__ */

