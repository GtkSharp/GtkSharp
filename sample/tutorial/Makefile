
SUBDIRS = helloworld \
		helloworld2 \
		base \
		arrow \
		aspectframe \
		filesel \
		frame \
		label \
		table \
		buttons \
		radiobuttons \
		scrolledwin \


all:
	list='$(SUBDIRS)'; \
	for subdir in $$list; do \
	  (cd $$subdir && $(MAKE)); \
	done

clean:
	list='$(SUBDIRS)'; \
	for subdir in $$list; do \
	  (cd $$subdir && $(MAKE) clean); \
	done

